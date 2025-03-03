﻿using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace Siccity.GLTFUtility {
	[ScriptedImporter(1, "gltf")]
	public class GLTFImporter : ScriptedImporter {

		public ImportSettings importSettings;

		public override void OnImportAsset(AssetImportContext ctx) {
			// Load asset
			GLTFAnimation.ImportResult[] animations;
			GameObject root = Importer.ImportGLTF(ctx.assetPath, importSettings, out animations);
			// Save asset
			GLTFAssetUtility.SaveToAsset(root, animations, ctx);
		}
	}
}