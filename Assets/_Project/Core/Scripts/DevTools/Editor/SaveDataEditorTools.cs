using System.IO;
using UnityEditor;
using UnityEngine;

namespace ANut.Core.DevTools.Editor
{
    public static class SaveDataEditorTools
    {
        private const string MenuRoot = "My Game/";

        [MenuItem(MenuRoot + "Delete All Save Data")]
        private static void DeleteAllSaveData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            string persistentPath = Application.persistentDataPath;
            int deletedCount = 0;

            foreach (string filePath in Directory.GetFiles(persistentPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    File.Delete(filePath);
                    Log.Info("[SaveDataEditorTools] Deleted: {0}", filePath);
                    deletedCount++;
                }
                catch (IOException e)
                {
                    Log.Warning("[SaveDataEditorTools] Could not delete {0}: {1}", filePath, e.Message);
                }
            }

            Log.Info("[SaveDataEditorTools] Done. Deleted {0} file(s) + PlayerPrefs.", deletedCount);
        }

        [MenuItem(MenuRoot + "Delete All Save Data", validate = true)]
        private static bool ValidateDeleteAllSaveData() => !EditorApplication.isPlaying;

        [MenuItem(MenuRoot + "Open Save Folder")]
        private static void OpenSaveFolder()
        {
            string path = Application.persistentDataPath;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            EditorUtility.RevealInFinder(path);
        }
    }
}