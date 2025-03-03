﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Siccity.GLTFUtility {
	// https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#image
	public class GLTFImage {

		public string uri;
		public string mimeType;
		public int? bufferView;
		public string name;

		public class ImportResult {
			public Texture2D texture;
			/// <summary> True if image was loaded from a Texture2D asset. False if it was loaded from binary or from another source </summary>
			public bool isAsset;
			public bool isNormalMap;
			public bool isMetallicRoughnessFixed;

			public Texture2D GetNormalMap() {
				if (isNormalMap || isAsset) return texture;
				Color32[] pixels = texture.GetPixels32();
				for (int i = 0; i < pixels.Length; i++) {
					Color32 c = pixels[i];
					c.a = pixels[i].r;
					c.r = c.b = c.g;
					pixels[i] = c;
				}
				texture.SetPixels32(pixels);
				texture.Apply();
				isNormalMap = true;
				return texture;
			}

			// glTF stores Metallic in blue channel and roughness in green channel. Unity stores Metallic in red and roughness in alpha. This method returns a unity-fixed texture
			public Texture2D GetFixedMetallicRoughness() {
				if (!isMetallicRoughnessFixed && !isAsset) {
					Color32[] pixels = texture.GetPixels32();
					for (int i = 0; i < pixels.Length; i++) {
						Color32 c = pixels[i];
						c.r = pixels[i].b;
						c.a = pixels[i].g;
						pixels[i] = c;
					}
					texture.SetPixels32(pixels);
					texture.Apply();
					isMetallicRoughnessFixed = true;
				}
				return texture;
			}
		}

		public ImportResult GetImage(string directoryRoot, GLTFBufferView.ImportResult[] bufferViews) {
			ImportResult result = new ImportResult();
			result.isAsset = false;

			if (!string.IsNullOrEmpty(uri) && File.Exists(directoryRoot + uri)) {
#if UNITY_EDITOR
				result.texture = UnityEditor.AssetDatabase.LoadAssetAtPath(directoryRoot + uri, typeof(Texture2D)) as Texture2D;
				if (result.texture != null) {
					result.isAsset = true;
					return result;
				}
#endif
				Debug.Log("Couldn't load texture at " + directoryRoot + uri);
				return null;
			} else if (bufferView.HasValue && !string.IsNullOrEmpty(mimeType)) {
				byte[] bytes = bufferViews[bufferView.Value].bytes;
				result.texture = new Texture2D(2, 2);
				// If this fails, you may need to find "Image Conversion" package and enable it
				if (result.texture.LoadImage(bytes)) {
					return result;
				} else {
					Debug.Log("mimeType not supported: " + mimeType);
					return null;
				}
			} else {
				Debug.Log("Couldn't find texture at " + directoryRoot + uri);
				return null;
			}
		}
	}

	public static class GLTFImageExtensions {
		public static GLTFImage.ImportResult[] Import(this List<GLTFImage> images, string directoryRoot, GLTFBufferView.ImportResult[] bufferViews) {
			if (images == null) return null;

			GLTFImage.ImportResult[] results = new GLTFImage.ImportResult[images.Count];
			for (int i = 0; i < images.Count; i++) {
				results[i] = images[i].GetImage(directoryRoot, bufferViews);
			}
			return results;
		}
	}
}