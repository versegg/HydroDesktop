﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Search3.Settings.UI
{
    public partial class KeywordsUserControl : UserControl
    {
        #region Fields

        private KeywordsSettings _keywordsSettings;

        #endregion

        #region Constructors

        public KeywordsUserControl()
        {
            InitializeComponent();

            // Keywords display
            rbList.Tag = DisplayMode.List;
            rbTree.Tag = DisplayMode.Tree;
            rbBoth.Tag = DisplayMode.Both;
            foreach (var rb in new[] {rbList, rbTree, rbBoth})
                rb.CheckedChanged += rb_CheckedChanged;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Set data into control.
        /// </summary>
        /// <param name="keywordsSettings">Keyword settings.</param>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="keywordsSettings"/> is null.</exception>
        public void BindKeywordsSettings(KeywordsSettings keywordsSettings)
        {
            if (keywordsSettings == null) throw new ArgumentNullException("keywordsSettings");

            _keywordsSettings = keywordsSettings;
            
            tboTypeKeyword.Clear();
            lblKeywordRelation.Text = "";
           
            // Keywords
            lbKeywords.DataSource = keywordsSettings.Keywords;
            
            // Ontology tree
            treeviewOntology.BeginUpdate();
            treeviewOntology.Nodes.Clear();
            FillTreeviewOntology(treeviewOntology.Nodes, keywordsSettings.OntologyTree.Nodes);
            treeviewOntology.EndUpdate();

            // Selected keywords
            AddSelectedKeywords(keywordsSettings.SelectedKeywords);
            if (keywordsSettings.SelectedKeywords.Any())
            {
                UpdateKeywordTextBox(keywordsSettings.SelectedKeywords.First());
            }
        }

        /// <summary>
        /// Add selected keywords.
        /// </summary>
        /// <param name="keywords">Keywords to add.</param>
        /// <exception cref="ArgumentNullException">Throws if <paramref name="keywords"/> is null.</exception>
        private void AddSelectedKeywords(IEnumerable<string> keywords)
        {
            if (keywords == null) throw new ArgumentNullException("keywords");

            foreach (var keyword in keywords.Where(keyword => !lbSelectedKeywords.Items.Contains(keyword)))
            {
                lbSelectedKeywords.Items.Add(keyword);
            }
        }

        /// <summary>
        /// Remove selected keywords.
        /// </summary>
        /// <param name="keywords">Keywords to remove. If parameter is null - removes all selected keywords.</param>
        private void RemoveSelectedKeywords(IEnumerable<string> keywords = null)
        {
            if (keywords == null)
            {
                // remove all keywords
                lbSelectedKeywords.Items.Clear();
            }else
            {
                foreach(var keyword in keywords)
                {
                    lbSelectedKeywords.Items.Remove(keyword);
                }
            }
        }

        /// <summary>
        /// Get selected keywords.
        /// </summary>
        /// <returns>Selected keywords.</returns>
        public IEnumerable<string> GetSelectedKeywords()
        {
            var res = new List<string>(lbSelectedKeywords.Items.Count);
            res.AddRange(from object item in lbSelectedKeywords.Items select item.ToString());
            return res;
        }

        #endregion

        #region Private methods

        private void FillTreeviewOntology(TreeNodeCollection treeNodeCollection, IEnumerable<OntologyNode> ontologyNodes)
        {
            foreach (var oNode in ontologyNodes)
            {
                var treeNode = CreateTreeNodeFromOntologyNode(oNode);
                treeNodeCollection.Add(treeNode);
                foreach (var oChild in oNode.Nodes)
                {
                    var childTreeNode = CreateTreeNodeFromOntologyNode(oChild);
                    treeNode.Nodes.Add(childTreeNode);
                    FillTreeviewOntology(childTreeNode.Nodes, oChild.Nodes);
                }
            }
        }

        private TreeNode CreateTreeNodeFromOntologyNode(OntologyNode oNode)
        {
            var res = new TreeNode(oNode.Text) { Tag = oNode };
            return res;
        }

        void rb_CheckedChanged(object sender, EventArgs e)
        {
            var displayMode = (DisplayMode)((RadioButton)sender).Tag;
            spcKey.Panel2Collapsed = displayMode == DisplayMode.List;
            spcKey.Panel1Collapsed = displayMode == DisplayMode.Tree;
        }

        private void tboTypeKeyword_TextChanged(object sender, EventArgs e)
        {
            //control display if user is in treeview mode
            if (rbTree.Checked)
            {
                rbList.Checked = true;
            }
            
            lbKeywords.ClearSelected();
            var searchString2 = tboTypeKeyword.Text;
            lbKeywords.SelectionMode = SelectionMode.MultiSimple;
            var x = -1;

            if (searchString2.Length != 0)
            {
                do
                {
                    x = lbKeywords.FindString(searchString2, x);
                    if (x != -1)
                    {
                        if (lbKeywords.SelectedIndices.Count > 0)
                        {
                            if (x == lbKeywords.SelectedIndices[0])
                                return;
                        }
                        lbKeywords.SetSelected(x, true);
                    }
                }
                while (x != -1);
            }
        }

        private void lbKeywords_MouseUp(object sender, MouseEventArgs e)
        {
            var keyIndex = lbKeywords.IndexFromPoint(e.Location);
            if (keyIndex < 0) return;

            tboTypeKeyword.Text = string.Empty;
            var strNode = lbKeywords.Items[keyIndex].ToString();

            //check for synonyms
            if (_keywordsSettings.Synonyms != null)
            {
                foreach (var ontoPath in _keywordsSettings.Synonyms)
                {
                    if (string.Equals(ontoPath.SearchableKeyword, strNode, StringComparison.InvariantCultureIgnoreCase))
                    {
                        UpdateKeywordTextBox(ontoPath.ConceptName);
                    }
                }
            }

            //check ends
            if (tboTypeKeyword.Text == "")
            {
                UpdateKeywordTextBox(lbKeywords.Items[keyIndex].ToString());
            }
        }

        private void UpdateKeywordTextBox(string text)
        {
            tboTypeKeyword.Text = text;
            FindInTreeView(treeviewOntology.Nodes, tboTypeKeyword.Text);
        }

        private void tvOntology_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateKeywordTextBox(e.Node.Text);
        }

        private void FindInTreeView(IEnumerable tncoll, string strNode)
        {
            foreach (TreeNode tnode in tncoll)
            {
                if (tnode.Text.ToLower() == strNode.ToLower())
                {
                    tnode.TreeView.SelectedNode = tnode;
                    lblKeywordRelation.Text = tnode.FullPath;
                }

                FindInTreeView(tnode.Nodes, strNode);
            }
        }

        private void btnAddKeyword_Click(object sender, EventArgs e)
        {
            if (lbKeywords.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a valid Keyword.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var itemToAdd = lbKeywords.SelectedItem.ToString();
            if (GetSelectedKeywords().Any(item => item == itemToAdd))
            {
                MessageBox.Show("This Keyword is already selected, Please select another keyword.", "Information",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            AddSelectedKeywords(new[] {itemToAdd});
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (lbSelectedKeywords.SelectedItems.Count <= 0)
                return;

            var itemsToRemove = new List<string>(lbSelectedKeywords.SelectedItems.Count);
            itemsToRemove.AddRange(from object selected in lbSelectedKeywords.SelectedItems select selected.ToString());
            RemoveSelectedKeywords(itemsToRemove);
        }
       
        #endregion

        #region Helpers

        private enum DisplayMode
        {
            List,
            Tree,
            Both
        }

        #endregion
    }
}
