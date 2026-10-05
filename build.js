const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');
const esbuild = require('esbuild');

console.log('[Build] Compiling Web Assets...');

const distDir = path.resolve('./dist-web');
if (!fs.existsSync(distDir)) {
  fs.mkdirSync(distDir, { recursive: true });
}

try {
  esbuild.buildSync({
    entryPoints: ['index.tsx'],
    bundle: true,
    minify: true,
    sourcemap: false,
    outfile: 'dist-web/index.js',
    platform: 'browser',
    target: ['chrome100'],
    define: {
      'process.env.NODE_ENV': '"production"'
    }
  });
  console.log('[Build] JavaScript bundle created at dist-web/index.js');
} catch (e) {
  console.error('[Build] JS compilation failed:', e.message);
  process.exit(1);
}

try {
  execSync('npx @tailwindcss/cli -i input.css -o dist-web/index.css --minify', { stdio: 'inherit' });
  console.log('[Build] Tailwind CSS compiled at dist-web/index.css');
} catch (e) {
  console.error('[Build] Tailwind compilation failed:', e.message);
  process.exit(1);
}

console.log('[Build] Web build complete successfully.');
