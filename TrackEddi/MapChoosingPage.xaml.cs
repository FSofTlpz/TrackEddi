using FSofTUtils.Xamarin.Control;
using GMap.NET.CoreExt.MapProviders;
using System;
using System.Collections.Generic;
using TrackEddi.Common;
using TrackEddi.ConfigEdit;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TrackEddi
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
   public partial class MapChoosingPage : ContentPage {


      public SpecialMapCtrl.SpecialMapCtrl MapControl;

      public List<int[]> ProvIdxPaths;

      public Config Config;

      public AppData AppData;

      List<int> lastusedmapsidx = new List<int>();



      class ListViewObjectItem {

         MapProviderDefinition mpd;

         public string Text {
            get => mpd.MapName + " (" + mpd.ProviderName + ")";
         }

         public ListViewObjectItem(MapProviderDefinition def) {
            mpd = def;
         }

         public override string ToString() {
            return string.Format("{0}", Text);
         }

      }

      List<ListViewObjectItem> listViewObjectItems;


      public class MapChoosingEventArgs : EventArgs {
         /// <summary>
         /// Index der ausgewählten <see cref="MapProviderDefinition"/>
         /// </summary>
         public int Idx { get; private set; }

         public MapChoosingEventArgs(int idx) {
            Idx = idx;
         }

      }

      /// <summary>
      /// Auswahl ist beendet
      /// </summary>
      public event EventHandler<MapChoosingEventArgs> MapChoosingEvent;



      public MapChoosingPage() {
         InitializeComponent();

         //tv.OnBeforeCheckedChanged += Tv_OnBeforeCheckedChanged;
         //tv.OnCheckedChanged += Tv_OnCheckedChanged;

         //tv.OnBeforeExpandedChanged += Tv_OnBeforeExpandedChanged;
         //tv.OnExpandedChanged += TV_OnExpandedChanged;

         //tv.OnSelectedNodeChanged += TV_OnSelectedNodeChanged;
         tv.OnNodeTapped += TV_OnNodeTapped;
         //tv.OnNodeDoubleTapped += TV_OnNodeDoubleTapped;

         //tv.OnNodeSwiped += TV_OnNodeSwiped;

      }

      protected override void OnAppearing() {
         base.OnAppearing();
         buildTreeViewContent(Config, AppData, tv, MapControl.SpecMapProviderDefinitions, ProvIdxPaths);
      }

      //void clearTreeViewNodes(IList<TreeViewNode> nodes) {
      //   if (nodes != null) {
      //      foreach (TreeViewNode node in nodes)
      //         if (node.HasChildNodes)
      //            clearTreeViewNodes(node.GetChildNodes());
      //      nodes.Clear();
      //   }
      //}

      //void clearTreeViewNodes(TreeView tv) {
      //   if (tv.HasChildNodes) {
      //      IList<TreeViewNode> nodes = tv.GetChildNodes();
      //      clearTreeViewNodes(nodes);
      //      nodes.Clear();
      //   }
      //}

      int getIdx4Mapname(string mapname, IList<MapProviderDefinition> providerdefs) {
         for (int j = 0; j < providerdefs.Count; j++)
            if (providerdefs[j].MapName == mapname)
               return j;
         return -1;
      }

      void buildTreeViewContent(Config config,
                                AppData appData,
                                TreeView tv,
                                IList<MapProviderDefinition> providerdefs,
                                IList<int[]> providxpaths) {
         MapTreeViewHelper.BuildTreeViewContent(config, tv, providerdefs, providxpaths, MapControl.SpecMapActualMapIdx);

         //int selectedIdx = MapControl.SpecMapActualMapIdx;

         //TreeViewNode nodeSelected = null;
         //clearTreeViewNodes(tv);
         //for (int provideridx = 0; provideridx < providerdefs.Count; provideridx++) {
         //   TreeViewNode nodeParent = null;
         //   List<TreeViewNode> nodes = tv.GetChildNodes();

         //   for (int level = 1; level < providxpaths[provideridx].Length; level++) {
         //      if (level < providxpaths[provideridx].Length - 1) {   // Providergroup
         //         int groupidx = providxpaths[provideridx][level];

         //         TreeViewNode node4group = null;
         //         int destidx = -1;
         //         for (int i = 0; i < nodes.Count; i++) {
         //            if (nodes[i].ExtendedData == null) {
         //               if (++destidx == groupidx) {
         //                  node4group = nodes[i];
         //                  break;
         //               }
         //            }
         //         }

         //         if (node4group == null) {
         //            TreeViewNode n = new TreeViewNode(config.ProviderGroupName(providxpaths[provideridx], level + 1));
         //            if (nodeParent == null)
         //               tv.AddChildNode(n);
         //            else
         //               nodeParent.AddChildNode(n);
         //            nodeParent = n;
         //         } else
         //            nodeParent = node4group;

         //      } else {                                              // Provideritem
         //         TreeViewNode n = new TreeViewNode(providerdefs[provideridx].MapName, provideridx);
         //         if (nodeParent == null)
         //            tv.AddChildNode(n);
         //         else
         //            nodeParent.AddChildNode(n);
         //         if (provideridx == selectedIdx)
         //            nodeSelected = n;
         //      }
         //   }
         //}

         //if (nodeSelected != null)
         //   tv.SelectedNode = nodeSelected;

         int lastusedmapsmax = Math.Max(0, config.LastUsedMapsCount);
         if (lastusedmapsmax > 0) {
            tv.InsertChildNode(0, new TreeViewNode("⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯⎯") { IsEnabled = false });
            lastusedmapsidx.Clear();
            List<string> mapnames = appData.LastUsedMapnames;
            if (mapnames != null) {
               while (lastusedmapsmax < mapnames.Count)
                  mapnames.RemoveAt(0);
               for (int i = 0; i < mapnames.Count; i++) {
                  int idx = getIdx4Mapname(mapnames[i], providerdefs);
                  if (0 <= idx) {
                     tv.InsertChildNode(0, new TreeViewNode(mapnames[i], idx));
                     lastusedmapsidx.Add(idx);
                  }
               }
            }
         }
      }
      //private void Tv_OnCheckedChanged(object sender, TreeView.TreeViewNodeEventArgs e) { }

      //private void Tv_OnBeforeCheckedChanged(object sender, TreeView.TreeViewNodeStatusChangedEventArgs e) { }

      //private void Tv_OnBeforeExpandedChanged(object sender, TreeView.TreeViewNodeStatusChangedEventArgs e) { }

      //private void TV_OnNodeSwiped(object sender, TreeView.TreeViewNodeEventArgs e) { }

      //private void TV_OnNodeDoubleTapped(object sender, TreeView.TreeViewNodeEventArgs e) { }

      //private void TV_OnSelectedNodeChanged(object sender, TreeView.TreeViewNodeChangedEventArgs e) { }

      //private void TV_OnExpandedChanged(object sender, TreeView.TreeViewNodeEventArgs e) { }

      private void TV_OnNodeTapped(object sender, TreeView.TreeViewNodeEventArgs e) {
         if (e.TreeViewNode.ExtendedData != null) {
            string mapname = e.TreeViewNode.Text;
            int idx = getIdx4Mapname(mapname, MapControl.SpecMapProviderDefinitions);
            if (idx != MapControl.SpecMapActualMapIdx) {
               List<string> mapnames = AppData.LastUsedMapnames;
               int m = mapnames.IndexOf(mapname);
               if (0 <= m)
                  mapnames.RemoveAt(m);
               mapnames.Add(mapname);
               while (Config.LastUsedMapsCount < mapnames.Count)
                  mapnames.RemoveAt(0);
               AppData.LastUsedMapnames = mapnames;
               AppData.LastMapname = mapname;
               Navigation.PopAsync();     // diese Seite sofort schließen
               MapChoosingEvent?.Invoke(this, new MapChoosingEventArgs(idx));
            }
         } else {
            if (e.TreeViewNode.HasChildNodes)
               e.TreeViewNode.Expanded = !e.TreeViewNode.Expanded;
         }
      }


   }
}