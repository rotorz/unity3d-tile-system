// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

"use strict";


var child_process = require('child_process');
const fs = require("fs-extra");
const nunjucks = require("nunjucks");
const path = require("path");


const projectRootDir = process.cwd();
const projectConfigPath = path.join(projectRootDir, "package.json");
const projectConfig = fs.readJsonSync(projectConfigPath);


var productInfo = {
  version: parseVersion(projectConfig.version),
  commit: gitLatestCommit()
};

var context = {
  product: productInfo,
  version: productInfo.version,
  commit: productInfo.commit
};

renderTemplateFile(
    path.join(__dirname, 'templates/CommonAssemblyInfo.cs.nunjucks'),
    path.join(__dirname, 'templates/Common/Properties/CommonAssemblyInfo.cs'),
    context
  );
renderTemplateFile(
    path.join(__dirname, 'templates/ProductInfo.cs.nunjucks'),
    path.join(__dirname, '../assets/Source/ProductInfo.cs'),
    context
  );



//-----------------------------------------------------------------------------
// Nunjucks Helpers
//-----------------------------------------------------------------------------

function renderTemplateFile(sourceFile, outputFile, context) {
	nunjucks.configure(path.dirname(sourceFile), { watch: false });
  var result = nunjucks.render(path.basename(sourceFile), context);

  fs.ensureDirSync(path.dirname(outputFile));
	fs.writeFileSync(outputFile, result);
}



//-----------------------------------------------------------------------------
// Version Helpers
//-----------------------------------------------------------------------------

function parseVersion(versionTag) {
	var matches = versionTag.match(/^([0-9]+)\.([0-9]+)\.([0-9]+)(-(.+))?/);
	return {
		'major': matches[1],
		'minor': matches[2],
		'patch': matches[3],
		'label': matches[5],
		'informational': versionTag
	};
}



//-----------------------------------------------------------------------------
// Git Helpers
//-----------------------------------------------------------------------------

function gitLatestCommitHash() {
  try {
    return child_process.execSync('git rev-parse HEAD').toString().trim();
  }
  catch (err) {
    console.error(err.toString());
    return "unknown";
  }
}

function gitLatestCommit() {
	var latestCommitHash = gitLatestCommitHash();
	if (!latestCommitHash) {
    return { };
  }
	return {
		longHash: latestCommitHash,
		shortHash: latestCommitHash.substr(0, 7)
	};
}
