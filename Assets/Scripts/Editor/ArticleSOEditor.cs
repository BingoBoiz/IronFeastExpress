using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ArticleSO))]
public class ArticleSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ArticleSO articleSO = (ArticleSO)target;

        // Display headline field
        articleSO.headline = EditorGUILayout.TextField("Headline", articleSO.headline);

        // Display content field with automatic line wrapping
        GUILayout.Label("Content:");
        var contentStyle = new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true
        };
        articleSO.content = EditorGUILayout.TextArea(articleSO.content, contentStyle, GUILayout.Height(100));

        // Display outcome data entries
        for (int i = 0; i < articleSO.articleOutcomeDataList.Count; i++)
        {
            var outcomeData = articleSO.articleOutcomeDataList[i];

            // Use a vertical layout for each outcome
            EditorGUILayout.BeginVertical("box");

            // Display Article Outcome field and Remove button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Article Outcome", GUILayout.Width(150));
            outcomeData.articleOutcome = (ArticleOutcomeSO)EditorGUILayout.ObjectField(outcomeData.articleOutcome, typeof(ArticleOutcomeSO), false, GUILayout.Width(200));

            // Remove button
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                articleSO.articleOutcomeDataList.RemoveAt(i);
                AdjustRemainingChancesAfterRemoval(articleSO);
                EditorUtility.SetDirty(articleSO);
                return; // Exit to avoid skipping items due to list modification
            }
            EditorGUILayout.EndHorizontal();

            // Display Chance field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Chance (%)", GUILayout.Width(150));
            int previousChance = outcomeData.outcomeChance;
            outcomeData.outcomeChance = EditorGUILayout.IntSlider(outcomeData.outcomeChance, 0, 100, GUILayout.Width(200));

            // Adjust remaining chances if the value changes
            if (previousChance != outcomeData.outcomeChance)
            {
                AdjustRemainingChances(articleSO, i, previousChance - outcomeData.outcomeChance);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // Add space between entries for better readability
            EditorGUILayout.Space();
        }

        // Add a new outcome data entry
        if (GUILayout.Button("Add Outcome"))
        {
            articleSO.articleOutcomeDataList.Add(new ArticleSO.ArticleOutcomeData());
            AdjustRemainingChancesAfterAddition(articleSO);
        }

        // Ensure outcome chances sum to 100%
        EnsureChancesSumTo100(articleSO);

        // Apply changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(articleSO);
        }
    }

    private void AdjustRemainingChances(ArticleSO articleSO, int changedIndex, int delta)
    {
        // Adjust the remaining chances after the changed index
        int remainingDelta = delta;
        for (int i = changedIndex + 1; i < articleSO.articleOutcomeDataList.Count && remainingDelta != 0; i++)
        {
            var outcomeData = articleSO.articleOutcomeDataList[i];
            int adjustedChance = outcomeData.outcomeChance + remainingDelta;

            if (adjustedChance < 0)
            {
                remainingDelta = adjustedChance;
                outcomeData.outcomeChance = 0;
            }
            else if (adjustedChance > 100)
            {
                remainingDelta = adjustedChance - 100;
                outcomeData.outcomeChance = 100;
            }
            else
            {
                outcomeData.outcomeChance = adjustedChance;
                remainingDelta = 0;
            }
        }

        // If there's still remaining delta, apply it backward
        if (remainingDelta != 0)
        {
            for (int i = changedIndex - 1; i >= 0 && remainingDelta != 0; i--)
            {
                var outcomeData = articleSO.articleOutcomeDataList[i];
                int adjustedChance = outcomeData.outcomeChance + remainingDelta;

                if (adjustedChance < 0)
                {
                    remainingDelta = adjustedChance;
                    outcomeData.outcomeChance = 0;
                }
                else if (adjustedChance > 100)
                {
                    remainingDelta = adjustedChance - 100;
                    outcomeData.outcomeChance = 100;
                }
                else
                {
                    outcomeData.outcomeChance = adjustedChance;
                    remainingDelta = 0;
                }
            }
        }
    }

    private void AdjustRemainingChancesAfterRemoval(ArticleSO articleSO)
    {
        EnsureChancesSumTo100(articleSO);
    }

    private void AdjustRemainingChancesAfterAddition(ArticleSO articleSO)
    {
        EnsureChancesSumTo100(articleSO);
    }

    private void EnsureChancesSumTo100(ArticleSO articleSO)
    {
        int totalChance = 0;
        foreach (var outcomeData in articleSO.articleOutcomeDataList)
        {
            totalChance += outcomeData.outcomeChance;
        }

        if (totalChance != 100 && articleSO.articleOutcomeDataList.Count > 0)
        {
            float scaleFactor = 100f / totalChance;
            for (int i = 0; i < articleSO.articleOutcomeDataList.Count; i++)
            {
                articleSO.articleOutcomeDataList[i].outcomeChance = Mathf.RoundToInt(articleSO.articleOutcomeDataList[i].outcomeChance * scaleFactor);
            }
        }
    }
}
