using System;
using UnityEngine;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToolbarControl_NS;

namespace ModList
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class ModListToolbar : MonoBehaviour
    {

        private ApplicationLauncherButton toolbar_button = null;
        private Texture2D icon_texture = null;

        private void Start()
        {
            ToolbarControl.RegisterMod("ModList", "ModList");
        }

        /*public void Create()
        {
            icon_texture = new Texture2D(24, 24);
           icon_texture.LoadImage(File.ReadAllBytes(KSPUtil.ApplicationRootPath + "GameData/ModList/Icon.png"));
            GameEvents.onGUIApplicationLauncherReady.Add(delegate
            {
                CreateStockToolbarButton();
            });
            GameEvents.onGUIApplicationLauncherUnreadifying.Add(delegate { DestroyStockToolbarButton(); });
        }


        private void CreateStockToolbarButton()
        {
            if (toolbar_button == null)
            {
                toolbar_button = ApplicationLauncher.Instance.AddModApplication(
                    OnTrue,
                    OnFalse,
                    null,
                    null,
                    null,
                    null,
                    ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
                    icon_texture
                );
            }
        }

        private void OnTrue()
        {
            ModList.isGUIenabled = true;
        }

        private void OnFalse()
        {
            ModList.isGUIenabled = false;
        }
        
        private void DestroyStockToolbarButton()
        {
            if (toolbar_button != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(toolbar_button);
                toolbar_button = null;
            }
            icon_texture = null;
        }*/
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class ModList : MonoBehaviour
    {
        public bool isGUIenabled = false;
        public bool isVisible;
        private PopupDialog dialog = null;
        private UrlDir.UrlConfig[] configs = null;
        private List<string> mods;
        private ToolbarControl toolbarControl = null;

        private Rect geometry;
        //private void Awake()
        //{
        //    ModListToolbar.Create();
        //}

        private void Update()
        {
            if (isGUIenabled && !isVisible)
            {
                Show();
            }
            else if (!isGUIenabled && isVisible)
            {
                Hide();
            }
        }

        private void Start()
        {
            CreateToolbarButton();
            geometry = new Rect(0.5f, 0.5f, 300f, 100f);
            GameEvents.onEditorPartPlaced.Add(OnVesselModified);
            GameEvents.onEditorPartPicked.Add(OnVesselModified);
        }

        private void OnVesselModified(Part p)
        {
            //if (!HighLogic.LoadedSceneIsEditor)
            //    return;
            Refresh();
        }

        private void OnTrue()
        {
            isGUIenabled = true;
        }

        private void OnFalse()
        {
            isGUIenabled = false;
        }

        private void CreateToolbarButton()
        {
            toolbarControl = gameObject.AddComponent<ToolbarControl>();
            toolbarControl.AddToAllToolbars(OnTrue, OnFalse, ApplicationLauncher.AppScenes.VAB | 
                                                             ApplicationLauncher.AppScenes.SPH, "ModList", 
                "ModListButton", "ModList/Icon.png", "ModList/Icon.png");
        }

        private void OnDestroy()
        {
            toolbarControl.OnDestroy();
            Destroy(toolbarControl);
            GameEvents.onEditorPartPlaced.Remove(OnVesselModified);
            GameEvents.onEditorPartPicked.Remove(OnVesselModified);
        }

        private void GetModList()
        {
            mods = new List<string>();
            // TODO : foreach -> for
            foreach (Part p in EditorLogic.SortedShipList)
            {
                mods.Add(FindPartMod(p));
            }

            mods = mods.Distinct().ToList();
        }

        private void Show()
        {
            if ((object) dialog != null)
                return;
            GetModList();

            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new MultiOptionDialog("ModList", String.Join("\n", mods), "Mod List",
                    HighLogic.UISkin, geometry, new []{new DialogGUIHorizontalLayout( new DialogGUIButton("Refresh", Refresh, 140f,40f,false), new DialogGUIButton("Copy To Clipboard", CopyToClipboard, 140f, 40f, false)), }), false, HighLogic.UISkin, false);
        
            //dialog.gameObject.SetActive(true);
            isVisible = true;
        }

        private void CopyToClipboard()
        {
            TextEditor t = new TextEditor {text = String.Join("\n", mods)};
            t.SelectAll();
            t.Copy();
        }

        private void Hide()
        {
            if ((object)dialog != null)
            {
                Vector3 rt = dialog.RTrf.position;
                geometry = new Rect(
                    rt.x / Screen.width  + 0.5f,
                    rt.y / Screen.height + 0.5f,
                    300f,
                    100f
                );
                dialog.Dismiss();
                Destroy(dialog);
                dialog = null;
                isVisible = false;
            }
        }

        private void Refresh()
        {
            Hide();
            Show();
        }
        
        private string FindPartMod(Part part)
        {
            if (configs == null)
                configs = GameDatabase.Instance.GetConfigs("PART");
            
            UrlDir.UrlConfig config = Array.Find<UrlDir.UrlConfig>(configs, (c => (part.name == c.name.Replace('_', '.').Replace(' ', '.'))));
            if (config == null)
            {
                config = Array.Find<UrlDir.UrlConfig>(configs, (c => (part.name == c.name)));
                if (config == null)
                    return "";
            }
            var id = new UrlDir.UrlIdentifier(config.url);
            if (id[0].Equals("SquadExpansion"))
            {
                if (id[1].Equals("Serenity"))
                {
                    return "BreakingGround";
                }

                return id[1];
            }

            if (id[0].Equals("UmbraSpaceIndustries") || id[0].Equals("WildBlueIndustries"))
            {
                return id[0] + "/" + id[1];
            }
            return id[0];
        }
        
    }
}