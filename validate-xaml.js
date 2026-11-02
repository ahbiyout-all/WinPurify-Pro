/**
 * WinPurify Pro - Preliminary XAML Validator
 * Validates XML well-formedness, tag closures, duplicate/empty keys,
 * and WPF ResourceDictionary hierarchy rules (catching MC3023 and syntax errors before compilation).
 */

const fs = require('fs');
const path = require('path');

function validateXamlFile(filePath) {
  const content = fs.readFileSync(filePath, 'utf8');
  const errors = [];
  const warnings = [];

  // 1. Remove XML comments while preserving line breaks to keep line numbers accurate
  let sanitized = content.replace(/<!--[\s\S]*?-->/g, (match) => {
    return match.replace(/[^\r\n]/g, ' ');
  });

  // Also mask CDATA blocks
  sanitized = sanitized.replace(/<!\[CDATA\[[\s\S]*?\]\]>/g, (match) => {
    return match.replace(/[^\r\n]/g, ' ');
  });

  const lines = sanitized.split(/\r?\n/);
  const elementStack = []; // { tag, line, col, isDictionary, keys: Map }

  function isDictionaryContainer(tagName) {
    if (!tagName) return false;
    const cleanTag = tagName.includes(':') ? tagName.split(':')[1] : tagName;
    return cleanTag === 'ResourceDictionary' ||
           cleanTag.endsWith('.Resources') ||
           cleanTag.endsWith('Dictionary');
  }

  // Proper state-machine parser that correctly handles '>' inside quoted attribute strings
  let i = 0;
  const len = sanitized.length;

  while (i < len) {
    if (sanitized[i] === '<') {
      const tagStartIndex = i;
      i++;

      // Check if processing instruction or doctype
      if (sanitized[i] === '?' || sanitized[i] === '!') {
        while (i < len && sanitized[i] !== '>') i++;
        i++;
        continue;
      }

      const isClosing = sanitized[i] === '/';
      if (isClosing) i++;

      // Read tag name
      let tagName = '';
      while (i < len && !/[\s\/>]/.test(sanitized[i])) {
        tagName += sanitized[i];
        i++;
      }

      if (!tagName) {
        continue;
      }

      // Read attributes until closing '>' outside quotes
      let attributes = '';
      let isSelfClosing = false;
      let inQuote = null;

      while (i < len) {
        const ch = sanitized[i];
        if (inQuote) {
          attributes += ch;
          if (ch === inQuote) {
            inQuote = null;
          }
        } else {
          if (ch === '"' || ch === "'") {
            inQuote = ch;
            attributes += ch;
          } else if (ch === '/' && sanitized[i + 1] === '>') {
            isSelfClosing = true;
            i += 2;
            break;
          } else if (ch === '>') {
            i++;
            break;
          } else {
            attributes += ch;
          }
        }
        i++;
      }

      const linesBefore = sanitized.substring(0, tagStartIndex).split(/\r?\n/);
      const lineNum = linesBefore.length;
      const colNum = linesBefore[linesBefore.length - 1].length + 1;

      if (isClosing) {
        if (elementStack.length === 0) {
          errors.push({
            line: lineNum,
            col: colNum,
            message: `Unexpected closing tag </${tagName}> without matching opening tag.`
          });
        } else {
          const top = elementStack.pop();
          if (top.tag !== tagName) {
            errors.push({
              line: lineNum,
              col: colNum,
              message: `Mismatched closing tag: expected </${top.tag}> (opened at line ${top.line}:${top.col}), but found </${tagName}>.`
            });
          }
        }
      } else if (isSelfClosing) {
        checkKeyAttribute(tagName, attributes, lineNum, colNum, elementStack, errors);
      } else {
        checkKeyAttribute(tagName, attributes, lineNum, colNum, elementStack, errors);
        const isDict = isDictionaryContainer(tagName);
        elementStack.push({
          tag: tagName,
          line: lineNum,
          col: colNum,
          isDictionary: isDict,
          keys: new Map()
        });
      }
    } else {
      i++;
    }
  }

  // Check unclosed tags on stack
  if (elementStack.length > 0) {
    while (elementStack.length > 0) {
      const unclosed = elementStack.pop();
      errors.push({
        line: unclosed.line,
        col: unclosed.col,
        message: `Unclosed tag <${unclosed.tag}> opened here was never closed before end of file.`
      });
    }
  }

  return { errors, warnings, lineCount: lines.length };
}

function checkKeyAttribute(rawTag, attributes, lineNum, colNum, elementStack, errors) {
  const keyMatch = attributes.match(/\b(?:x:)?Key\s*=\s*(?:"([^"]*)"|'([^']*)')/);
  if (!keyMatch) return;

  const keyValue = keyMatch[1] || keyMatch[2];

  if (!keyValue || !keyValue.trim()) {
    errors.push({
      line: lineNum,
      col: colNum,
      code: 'EmptyKey',
      message: `[Invalid Key Violation] Tag '<${rawTag}>' defines an empty or whitespace-only Key attribute. ResourceDictionary keys must be non-empty.`
    });
    return;
  }

  const parent = elementStack[elementStack.length - 1];
  if (!parent) return;

  if (!parent.isDictionary) {
    errors.push({
      line: lineNum,
      col: colNum,
      code: 'MC3023',
      message: `[MC3023 Violation] Tag '<${rawTag}>' defines Key="${keyValue}" but is nested inside non-dictionary element '<${parent.tag}>' (opened at line ${parent.line}:${parent.col}). The Key attribute can only be used on elements contained directly within a ResourceDictionary or <*.Resources> tag. Check if a preceding tag (e.g. </Setter>, </Style>) was left unclosed!`
    });
  } else {
    // Check duplicate keys
    if (parent.keys.has(keyValue)) {
      const prevLine = parent.keys.get(keyValue);
      errors.push({
        line: lineNum,
        col: colNum,
        code: 'DuplicateKey',
        message: `[Duplicate Key Violation] Duplicate ResourceDictionary Key="${keyValue}" detected on '<${rawTag}>'. Already declared at line ${prevLine}. Duplicate keys will cause runtime initialization crash.`
      });
    } else {
      parent.keys.set(keyValue, lineNum);
    }
  }
}

function validateAllXaml(rootDir) {
  const xamlFiles = [];

  function scan(dir) {
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    for (const ent of entries) {
      if (ent.isDirectory()) {
        if (ent.name === 'node_modules' || ent.name === 'bin' || ent.name === 'obj' || ent.name === 'publish' || ent.name === '.git' || ent.name === 'dist-web') {
          continue;
        }
        scan(path.join(dir, ent.name));
      } else if (ent.isFile() && ent.name.toLowerCase().endsWith('.xaml')) {
        xamlFiles.push(path.join(dir, ent.name));
      }
    }
  }

  scan(rootDir);

  console.log('=====================================================================');
  console.log('   WinPurify Pro Pre-Build XAML Hook (Node.js Engine)');
  console.log(`   Target: Validating ${xamlFiles.length} XAML file(s) for Dictionary Keys & Syntax`);
  console.log('=====================================================================');

  let totalErrors = 0;

  for (const file of xamlFiles) {
    const relPath = path.relative(rootDir, file).replace(/\\/g, '/');
    try {
      const res = validateXamlFile(file);
      if (res.errors.length === 0) {
        console.log(`[PASS] ${relPath} (${res.lineCount} lines) - OK`);
      } else {
        totalErrors += res.errors.length;
        console.error(`\n[FAIL] ${relPath}: ${res.errors.length} error(s) detected:`);
        for (const err of res.errors) {
          const codeTag = err.code ? `[${err.code}] ` : '';
          console.error(`  --> Line ${err.line}, Col ${err.col}: ${codeTag}${err.message}`);
        }
        console.error('');
      }
    } catch (e) {
      totalErrors++;
      console.error(`[ERROR] Failed to parse ${relPath}: ${e.message}`);
    }
  }

  console.log('---------------------------------------------------------------------');
  if (totalErrors === 0) {
    console.log('[SUCCESS] All XAML files passed Pre-Build Dictionary Key & Syntax checks! Ready for compilation.\n');
    return true;
  } else {
    console.error(`[FATAL] Pre-build hook detected ${totalErrors} invalid XAML dictionary key / syntax error(s)!`);
    console.error('HALTING BUILD PIPELINE before MSVC C++ and .NET compilation to prevent wasted build cycles.\n');
    return false;
  }
}

if (require.main === module) {
  const targetDir = process.argv[2] ? path.resolve(process.argv[2]) : path.resolve(__dirname);
  const success = validateAllXaml(targetDir);
  process.exit(success ? 0 : 1);
}

module.exports = { validateXamlFile, validateAllXaml };
