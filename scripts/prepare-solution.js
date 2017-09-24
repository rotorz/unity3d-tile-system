// Copyright (c) Rotorz Limited. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root.

"use strict";


const fs = require("fs-extra");
const glob = require("glob");
const nunjucks = require("nunjucks");
const path = require("path");


const projectRootDir = process.cwd();
const projectConfigPath = path.join(projectRootDir, "package.json");
const projectConfig = fs.readJsonSync(projectConfigPath);


//-----------------------------------------------------------------------------
// Get list of package paths that have the keyword 'unity3d-package'.
//-----------------------------------------------------------------------------

const specialKeyword = "unity3d-package";

let packagePaths = [ path.join(projectRootDir, "assets") ];

const projectDependencies = getPackageListing(path.resolve(projectRootDir, "node_modules"));
const projectDependencyNames = new Set(projectDependencies);
for (let dependencyName of projectDependencyNames) {
  const dependencyDir = path.resolve(projectRootDir, "node_modules", dependencyName);
  const dependencyConfigPath = path.join(dependencyDir, "package.json");
  const dependencyConfig = fs.readJsonSync(dependencyConfigPath);
  const dependencyKeywords = dependencyConfig.keywords || [ ];

  // Skip packages that do not need to be installed using this mechanism.
  if (!dependencyKeywords.includes(specialKeyword)) {
    continue;
  }

  packagePaths.push(path.join(dependencyDir, "assets"));
}


//-----------------------------------------------------------------------------
// For each package; produce a list of runtime and editor source files.
//-----------------------------------------------------------------------------

let runtimeSources = [];
let editorSources = [];

for (let packagePath of packagePaths) {
  runtimeSources = runtimeSources.concat(getSources(path.join(packagePath, "Source")));
  editorSources = editorSources.concat(getSources(path.join(packagePath, "Editor")));
}


// Remove 'UnityIntegration.cs' from list of editor sources since this must be a loose
// source file in the Unity project.
editorSources = editorSources.filter(source => path.basename(source) !== "UnityIntegration.cs");


//-----------------------------------------------------------------------------
// Produce .csproj files from templates.
//-----------------------------------------------------------------------------

let outputSolutionPath = path.join(projectRootDir, "temp");

let projects = [
  { name: "TileSystem.Runtime_Editor", sources: runtimeSources },
  { name: "TileSystem.Runtime_Standalone", sources: runtimeSources },
  { name: "TileSystem.Editor", sources: editorSources },
];

for (let project of projects) {
  let projectTemplateDir = path.join(__dirname, "./templates/" + project.name);
  let projectTemplatePath = projectTemplateDir + ".csproj.nunjucks";
  let text = nunjucks.render(projectTemplatePath, project);

  let projectOutputDir = path.join(outputSolutionPath, project.name);
  fs.copySync(projectTemplateDir, projectOutputDir);
  let projectOutputPath = path.join(projectOutputDir, project.name + ".csproj");
  fs.writeFileSync(projectOutputPath, text, "utf8");
}

let commonTemplateDir = path.join(__dirname, "./templates/Common");
let commonOutputPath = path.join(outputSolutionPath, "Common");
fs.copySync(commonTemplateDir, commonOutputPath);

let solutionTemplatePath = path.join(__dirname, "./templates/TileSystem.sln");
let solutionOutputPath = path.join(outputSolutionPath, "TileSystem.sln");
fs.copySync(solutionTemplatePath, solutionOutputPath);


//-----------------------------------------------------------------------------
// Copy template 'Deploy' directory.
//-----------------------------------------------------------------------------

let deployTemplateDir = path.resolve(projectRootDir, "scripts/templates/Deploy");
let deployDir = path.resolve(projectRootDir, "temp/Deploy");
fs.copySync(deployTemplateDir, deployDir);

copySourceToDeploy(path.resolve(projectRootDir, "assets"), "unity3d-tile-system", "Editor/Generated");
copySourceToDeploy(path.resolve(projectRootDir, "assets"), "unity3d-tile-system", "Editor/Skin");
copySourceToDeploy(path.resolve(projectRootDir, "assets"), "unity3d-tile-system", "Languages");
copySourceToDeploy(path.resolve(projectRootDir, "assets"), "unity3d-tile-system", "Shaders");

copySourceToDeploy(path.resolve(projectRootDir, "node_modules/@rotorz/unity3d-reorderable-list/assets"), "unity3d-reorderable-list", "Editor/Skin");
copySourceToDeploy(path.resolve(projectRootDir, "node_modules/@rotorz/unity3d-utils/assets"), "unity3d-utils", "Editor/Skin");


//-----------------------------------------------------------------------------
// Substitute script GUIDs for DLL-hosted GUIDs.
//-----------------------------------------------------------------------------

substituteScriptReferenceInAsset(
    path.resolve(deployDir, "Assets/Plugins/Packages/@rotorz/unity3d-tile-system/Editor/Skin/Skin.asset"),
    "1036350792", "ca8d1606cd5c9cf4680b1ed766df048c"
  );
substituteScriptReferenceInAsset(
    path.resolve(deployDir, "Assets/Plugins/Packages/@rotorz/unity3d-reorderable-list/Editor/Skin/ReorderableListStyles.asset"),
    "1274629499", "ca8d1606cd5c9cf4680b1ed766df048c"
  );
substituteScriptReferenceInAsset(
    path.resolve(deployDir, "Assets/Plugins/Packages/@rotorz/unity3d-utils/Editor/Skin/ExtraEditorStyles.asset"),
    "993050685", "ca8d1606cd5c9cf4680b1ed766df048c"
  );
  

//-----------------------------------------------------------------------------
// Helper functions:
//-----------------------------------------------------------------------------

function getPackageListing(dir) {
  return flatMap(getDirectories(dir), packageDirectory =>
    packageDirectory.startsWith("@")
      ? getDirectories(path.join(dir, packageDirectory))
          .map(scopedPackageName => packageDirectory + "/" + scopedPackageName)
      : packageDirectory
  )
  .filter(dir => !path.basename(dir).startsWith("."));
}

function getSources(dir) {
  return glob.sync(path.join(dir, "**/*.cs"))
    .map(source => path.resolve(source));
}


function copySourceToDeploy(sourceDir, packageName, relativePath) {
  let sourcePath = path.resolve(sourceDir, relativePath);
  let outputPath = path.resolve(deployDir, "Assets/Plugins/Packages/@rotorz", packageName, relativePath);

  fs.ensureDirSync(outputPath);
  fs.copySync(sourcePath, outputPath);
  fs.copySync(sourcePath + ".meta", outputPath + ".meta");
}

function substituteScriptReferenceInAsset(assetFilePath, fileID, guid) {
  let asset = fs.readFileSync(assetFilePath, "utf8");
  asset = asset.replace(/m_Script:[^}]+}/, `m_Script: {fileID: ${fileID}, guid: ${guid}, type: 3}`);

  fs.writeFileSync(assetFilePath, asset, "utf8");
}


// Copied from: http://stackoverflow.com/questions/10865025/merge-flatten-an-array-of-arrays-in-javascript
function flatMap(a, cb) {
  return [].concat(...a.map(cb));
}

// Copied from: http://stackoverflow.com/a/24594123/656172
function getDirectories(srcpath) {
  return fs.readdirSync(srcpath)
  .filter(file => fs.statSync(path.join(srcpath, file)).isDirectory())
}
