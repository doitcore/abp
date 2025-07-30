const fse = require('fs-extra');
const { execaSync } = require('execa');

fse.copyFileSync('./package.json', './projects/utils/package.json');
fse.copyFileSync('./README.md', './projects/utils/README.md');

try {
  execaSync('yarn', ['build'], { stdio: 'inherit' });
  process.exit(0);
} catch (error) {
  console.error(error);
  process.exit(1);
}
