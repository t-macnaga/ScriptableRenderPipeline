using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace UnityEditor.Rendering.HighDefinition
{
    public enum ShaderID
    {
        Lit,
        LitTesselation,
        LayeredLit,
        LayeredLitTesselation,
        StackLit,
        Unlit,
        Fabric,
        Decal,
        TerrainLit,
        Count_Standard,
        SG_Unlit = Count_Standard,
        SG_Lit,
        SG_Hair,
        SG_Fabric,
        SG_StackLit,
        SG_Decal,
        Count_All,
        Count_ShaderGraph = Count_All - Count_Standard
    }

    public class HDShaderUtils
    {
        delegate void MaterialResetter(Material material);
        static Dictionary<string, MaterialResetter> k_MaterialResetters = new Dictionary<string, MaterialResetter>()
        {
            { "HDRP/LayeredLit",  LayeredLitGUI.SetupMaterialKeywordsAndPass },
            { "HDRP/LayeredLitTessellation", LayeredLitGUI.SetupMaterialKeywordsAndPass },
            { "HDRP/Lit", LitGUI.SetupMaterialKeywordsAndPass },
            { "HDRP/LitTessellation", LitGUI.SetupMaterialKeywordsAndPass },
            { "HDRP/Unlit", UnlitGUI.SetupUnlitMaterialKeywordsAndPass },
            { "HDRP/Decal", DecalUI.SetupMaterialKeywordsAndPass },
            { "HDRP/TerrainLit", TerrainLitGUI.SetupMaterialKeywordsAndPass },
        };

        static Dictionary<Type, MaterialResetter> k_ShaderGraphMaterialResetters = new Dictionary<Type, MaterialResetter>
        {
            { typeof(HDUnlitMasterNode), UnlitGUI.SetupUnlitMaterialKeywordsAndPass },
            { typeof(HDLitMasterNode), HDLitGUI.SetupMaterialKeywordsAndPass },
            { typeof(FabricMasterNode), FabricGUI.SetupMaterialKeywordsAndPass },
            { typeof(HairMasterNode), HairGUI.SetupMaterialKeywordsAndPass },
            { typeof(StackLitMasterNode), StackLitGUI.SetupMaterialKeywordsAndPass },
        };

        /// <summary>
        /// Reset the dedicated Keyword and Pass regarding the shader kind.
        /// Also re-init the drawers and set the material dirty for the engine.
        /// </summary>
        /// <param name="material">The material that nees to be setup</param>
        /// <returns>
        /// True: managed to do the operation.
        /// False: unknown shader used in material
        /// </returns>
        public static bool ResetMaterialKeywords(Material material)
        {
            MaterialResetter resetter = null;

            // For shader graphs, we retrieve the master node type to get the materials resetter
            if (material.shader.IsShaderGraph())
            {
                Type masterNodeType = null;
                try
                {
                    // GraphUtil.GetOutputNodeType can throw if it's not able to parse the graph
                    masterNodeType = GraphUtil.GetOutputNodeType(AssetDatabase.GetAssetPath(material.shader));
                }
                catch { }

                if (masterNodeType != null)
                {
                    k_ShaderGraphMaterialResetters.TryGetValue(masterNodeType, out resetter);
                }
            }
            else
            {
                k_MaterialResetters.TryGetValue(material.shader.name, out resetter);
            }

            if (resetter != null)
            {
                CoreEditorUtils.RemoveMaterialKeywords(material);
                // We need to reapply ToggleOff/Toggle keyword after reset via ApplyMaterialPropertyDrawers
                MaterialEditor.ApplyMaterialPropertyDrawers(material);
                resetter(material);
                EditorUtility.SetDirty(material);
                return true;
            }

            return false;
        }

        /// <summary>Gather all the shader preprocessors</summary>
        /// <returns>The list of shader preprocessor</returns>
        internal static List<BaseShaderPreprocessor> GetBaseShaderPreprocessorList()
        {
            var baseType = typeof(BaseShaderPreprocessor);
            var assembly = baseType.Assembly;

            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes()
                    .Where(t => t.IsSubclassOf(baseType))
                    .Select(Activator.CreateInstance)
                    .Cast<BaseShaderPreprocessor>()
                ).ToList();

            return types;
        }


        internal static bool IsHDRPShader(Shader shader, bool upgradable = false)
        {
            if (shader.IsShaderGraph())
            {
                var outputNodeType = GraphUtil.GetOutputNodeType(AssetDatabase.GetAssetPath(shader));
                return s_MasterNodes.Contains(outputNodeType);
            }
            else if (upgradable)
                return s_ShaderPaths.Contains(shader.name);
            else
                return shader.name.Contains("HDRP");
        }

        static readonly string[] s_ShaderPaths =
        {
            "HDRP/Lit",
            "HDRP/LitTessellation",
            "HDRP/LayeredLit",
            "HDRP/LayeredLitTessellation",
            "HDRP/StackLit",
            "HDRP/Unlit",
            "HDRP/Fabric",
            "HDRP/Decal",
            "HDRP/TerrainLit",
        };

        static readonly Type[] s_MasterNodes =
        {
            typeof(HDUnlitMasterNode),
            typeof(HDLitMasterNode),
            typeof(HairMasterNode),
            typeof(FabricMasterNode),
            typeof(StackLitMasterNode),
            typeof(DecalMasterNode),
        };

        internal static string GetShaderPath(ShaderID id)
        {
            int index = (int)id;
            if (index < 0 && index >= (int)ShaderID.Count_Standard)
            {
                Debug.LogError("Trying to access HDRP shader path out of bounds");
                return "";
            }

            return s_ShaderPaths[index];
        }

        internal static Type GetShaderMasterNodeType(ShaderID id)
        {
            int index = (int)id - (int)ShaderID.Count_Standard;
            if (index < 0 && index >= (int)ShaderID.Count_ShaderGraph)
            {
                Debug.LogError("Trying to access HDRP shader path out of bounds");
                return null;
            }

            return s_MasterNodes[index];
        }

        internal static ShaderID GetShaderEnumFromShader(Shader shader)
        {
            if (shader.IsShaderGraph())
            {
                var type = GraphUtil.GetOutputNodeType(AssetDatabase.GetAssetPath(shader));
                var index = Array.FindIndex(s_MasterNodes, m => m == type);
                return (ShaderID)(index + ShaderID.Count_Standard);
            }
            else
            {
                var index = Array.FindIndex(s_ShaderPaths, m => m == shader.name);
                return (ShaderID)index;
            }
        }
    }
}
