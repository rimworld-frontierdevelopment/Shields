using System;
using System.Xml;
using HarmonyLib;
using Verse;

namespace FrontierDevelopments.Shields.Harmony
{
    public static class Harmony_Map
    {
        [HarmonyPatch(typeof(Map), "ExposeComponents")]
        static class Patch_ExposeComponents
        {
            // this patch only exists to suppress errors with the existing. this time bombs the method after 1 year of
            // author date to prevent possible compatibility problems when it saves with the old format are unlikely to
            // exist anymore
            [HarmonyPrepare]
            static bool DisableWhenOld()
            {
                var expirationDate = new DateTime(2021, 10, 14);
                return DateTime.Now.CompareTo(expirationDate) < 0;
            }
            
            [HarmonyPrefix]
            static void RemoveShieldMapComponent()
            {
                if (Scribe.mode == LoadSaveMode.LoadingVars)
                {
                    var componentsNode = FindComponents(Scribe.loader.curXmlParent);
                    var shieldMapComponent = FindShieldMapComponent(componentsNode.ChildNodes);
                    if (shieldMapComponent != null)
                    {
                        componentsNode.RemoveChild(shieldMapComponent);
                    }
                }
            }
        }
        
        private static XmlNode FindComponents(XmlNode mapNode)
        {
            var mapChildrenNodes = mapNode.ChildNodes;
            for (var i = 0; i < mapChildrenNodes.Count; i++)
            {
                var mapChildNode = mapChildrenNodes.Item(i);
                if (mapChildNode == null) continue;
                if (mapChildNode.Name == "components")
                {
                    return mapChildNode;
                }
            }

            return null;
        }

        private static XmlNode FindShieldMapComponent(XmlNodeList mapComponentsNode)
        {
            if (mapComponentsNode == null) return null;
            for (var i = 0; i < mapComponentsNode.Count; i++)
            {
                var mapComponent = mapComponentsNode.Item(i);
                var attributes = mapComponent?.Attributes;
                if (attributes == null) continue;

                for (var j = 0; j < attributes.Count; j++)
                {
                    var attribute = attributes.Item(j);
                    if (attribute.Name == "Class" && attribute.Value == "FrontierDevelopments.Shields.ShieldManager")
                    {
                        return mapComponent;
                    }
                }
            }

            return null;
        }
    }
}