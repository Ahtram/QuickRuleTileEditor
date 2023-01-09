﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace QuickRuleTileEditor
{
    [System.Serializable]
    public class PatternedRuleTileEditModel
    {
        [SerializeField] private List<Sprite> sprites = new();
        [SerializeField] private int selectedTile = 0;
        [SerializeField] private List<RuleTile.TilingRuleOutput> tiles = new();
        [SerializeField] private RuleTile pattern;
        [SerializeField] private string patternId;
        [SerializeField] private RuleTile tileToEdit;

        public int SelectedTile
        {
            get => selectedTile;
            set => selectedTile = value;
        }
        public RuleTile TileToEdit => tileToEdit;
        public IReadOnlyList<Sprite> Sprites => sprites;
        public IEnumerable<RuleTile.TilingRuleOutput> AllOutputs => tiles;
        public int TilesCount => tiles.Count;
        public int PatternSize => pattern.m_TilingRules.Count;
        public string PatternId => patternId;
        public RuleTilePattern Pattern => new RuleTilePattern(patternId, pattern);


        public PatternedRuleTileEditModel() { }

        public PatternedRuleTileEditModel(RuleTilePattern pattern)
        {
            SetPattern(pattern);
        }

        public PatternedRuleTileEditModel(RuleTilePattern pattern, RuleTile tileToEdit)
        {
            this.tileToEdit = tileToEdit;
            tiles.AddRange(tileToEdit.m_TilingRules.Select(r => r.Clone()));

            SetPattern(pattern);
        }


        public void SetPattern(RuleTilePattern pattern)
        {
            patternId = pattern.Id;
            this.pattern = pattern.RuleTile;
            selectedTile = 0;
            var oldTilesCount = TilesCount;

            var existingTileSpritesCount = 0;
            for (int i = tiles.Count - 1; i >= 0; i--)
            {
                if (GetTileSprite(i) != null)
                {
                    existingTileSpritesCount = i + 1;
                    break;
                }
            }

            var tilesCount = Mathf.Max(pattern.RuleTile.m_TilingRules.Count, existingTileSpritesCount);

            for (int i = oldTilesCount - 1; i >= tilesCount; i--)
            {
                tiles.RemoveAt(i);
            }
            for (int i = 0; i < tilesCount - oldTilesCount; i++)
            {
                var index = tiles.Count;
                RuleTile.TilingRuleOutput ruleOutput;
                if (pattern.RuleTile.m_TilingRules.Count < index)
                {
                    ruleOutput = pattern.RuleTile.m_TilingRules[index].Clone();
                }
                else
                {
                    ruleOutput = new RuleTile.TilingRuleOutput();
                }
                tiles.Add(ruleOutput);
            }
        }

        public Sprite GetPatternSprite(int index)
        {
            if (index < 0 || index >= PatternSize) return null;
            return pattern.m_TilingRules[index].m_Sprites[0];
        }

        public void SetSpriteForTile(int tileIndex, Sprite sprite)
        {
            tiles[tileIndex].m_Sprites[0] = sprite;
        }

        public Sprite GetTileSprite(int tileIndex)
        {
            return tiles[tileIndex].m_Sprites[0];
        }

        public RuleTile.TilingRuleOutput GetTileOutput(int tileIndex)
        {
            return tiles[tileIndex];
        }

        public int SelectNextTile()
        {
            selectedTile = (selectedTile + 1) % PatternSize;
            return selectedTile;
        }

        public void AddSprite(Sprite sprite)
        {
            sprites.Add(sprite);
        }

        public void RemoveSprite(Sprite sprite)
        {
            sprites.Remove(sprite);
        }

        public void RemoveSpriteAt(int index)
        {
            sprites.RemoveAt(index);
        }

        public void ClearSprites()
        {
            sprites.Clear();
        }


        public void GenerateRuleTileAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save Rule Tile", "Rule Tile", "asset", "Message");
            if (string.IsNullOrEmpty(path)) return;

            var tile = ScriptableObject.CreateInstance<RuleTile>();

            for (int i = 0; i < PatternSize; i++)
            {
                var ruleOutput = tiles[i];

                var rule = pattern.m_TilingRules[i].Clone();
                rule.m_Sprites = (Sprite[])ruleOutput.m_Sprites.Clone();
                rule.m_GameObject = ruleOutput.m_GameObject;
                rule.m_ColliderType = ruleOutput.m_ColliderType;

                tile.m_TilingRules.Add(rule);
            }

            AssetDatabase.CreateAsset(tile, path);

            tileToEdit = tile;
        }

        public void SaveRuleTileAsset()
        {
            var targetRulesList = tileToEdit.m_TilingRules;
            for (int i = 0; i < PatternSize; i++)
            {
                var ruleOutput = tiles[i];
                RuleTile.TilingRule rule;

                if (targetRulesList.Count > i)
                {
                    rule = targetRulesList[i];
                }
                else
                {
                    rule = pattern.m_TilingRules[i].Clone();
                    targetRulesList.Add(rule);
                }
                rule.m_NeighborPositions.Clear();
                rule.m_NeighborPositions.AddRange(pattern.m_TilingRules[i].m_NeighborPositions);
                rule.m_Neighbors.Clear();
                rule.m_Neighbors.AddRange(pattern.m_TilingRules[i].m_Neighbors);
                rule.m_Sprites = (Sprite[])ruleOutput.m_Sprites.Clone();
                rule.m_GameObject = ruleOutput.m_GameObject;
                rule.m_ColliderType = ruleOutput.m_ColliderType;
            }

            for (int i = PatternSize; i < targetRulesList.Count; i++)
            {
                targetRulesList.RemoveAt(i);
                i--;
            }
        }
    }
}