using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace KAI
{
    public class ModelModifier : EditorWindow
    {
        private static ModelModifier Instance;

        private string[] PivotTypes = { "No Change", "Center", "Center Top", "Center Bottom", "Center Right", "Center Left", "Center Front", "Center Back" };
        private int CurrentPivotType;

        private GameObject Model;
        private GameObject NewRoot;
        private string NewRootName = "";
        private PIVOT_TYPE NewPivot;

        private bool UseNewScale;
        private Vector3 NewScale;

        private bool UseNewRotation;
        private Vector3 NewRotation;

        private ListRequest listRequest;
        private AddRequest addRequest;
        private bool CheckComplete = false;
        private bool AttemptingToAdd = false;
        private bool ExporterFound = false;

        [MenuItem("Custom Tools/Model Modifier")]
        public static void OpenWindow()
        {
            if (Instance == null)
            {
                Instance = GetWindow<ModelModifier>();
                Instance.minSize = new Vector2(640, 480);
            }
            else
            {
                EditorWindow.FocusWindowIfItsOpen<ModelModifier>();
            }

            Instance.titleContent = new GUIContent("Model Modifier");
        }


        private void OnEnable()
        {
            listRequest = Client.List();
            EditorApplication.update += CheckFBXExporter;
        }

        private void CheckFBXExporter()
        {
            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                    foreach (var package in listRequest.Result)
                    {
                        if (package.name.Equals("com.unity.formats.fbx"))
                        {
                            ExporterFound = true;
                            return;
                        }
                    }
                else if (listRequest.Status >= StatusCode.Failure)
                    Debug.Log(listRequest.Error.message);

                EditorApplication.update -= CheckFBXExporter;
                CheckComplete = true;
            }
        }

        private void AddFBXExporter()
        {
            if (addRequest.IsCompleted)
            {
                if (addRequest.Status == StatusCode.Success)
                    ExporterFound = true;
                else if (addRequest.Status >= StatusCode.Failure)
                    Debug.Log(listRequest.Error.message);

                EditorApplication.update -= AddFBXExporter;
                CheckComplete = true;
            }
        }

        private void OnInspectorUpdate()
        {
            if (ExporterFound == false && AttemptingToAdd == false)
            {
                addRequest = Client.Add("com.unity.formats.fbx");
                AttemptingToAdd = true;
                EditorApplication.update += AddFBXExporter;
            }
        }

        private void OnGUI()
        {
            if (ExporterFound)
            {
                DrawModelSection();
                DrawCustomizationSection();

                if (Model)
                {
                    GUILayout.BeginArea(new Rect(5, position.height - 60, position.width - 12, 60));

                    if (GUILayout.Button("Apply Settings"))
                    {
                        CreateModel();
                    }

                    GUILayout.Space(5);

                    if (GUILayout.Button("Export Model To FBX File"))
                    {
                        if (NewRoot == null)
                        {
                            CreateModel();
                        }

                        if (NewRoot)
                        {
                            Selection.activeGameObject = NewRoot;

                            EditorApplication.ExecuteMenuItem("GameObject/Export To FBX...");
                        }
                    }

                    GUILayout.EndArea();
                }
            }
            else
            {
                GUIStyle s = new GUIStyle();
                s.fontSize = 20;
                s.wordWrap = true;
                EditorGUILayout.LabelField("ATTEMPTING TO IMPORT FBX Exporter please check your internet connection", s, GUILayout.Width(position.width - 10), GUILayout.Height(100));
            }
        }

        private void DrawModelSection()
        {
            GUILayout.BeginArea(new Rect(2, 3, position.width - 6, 27), EditorStyles.helpBox);
            GUILayout.Space(1);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Model To Modify", EditorStyles.helpBox, GUILayout.Width(((position.width - 6) / 4)));
            Model = (GameObject)EditorGUILayout.ObjectField(Model, typeof(GameObject), true, GUILayout.Width(((position.width - 6) * 3 / 4) - 15));
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawCustomizationSection()
        {
            GUILayout.BeginArea(new Rect(2, 35, position.width - 6, position.height - 40), EditorStyles.helpBox);
            GUILayout.Space(1);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New Root Name", EditorStyles.helpBox, GUILayout.Width(((position.width - 6) / 4)));
            NewRootName = EditorGUILayout.TextField(NewRootName, EditorStyles.textField, GUILayout.Width(((position.width - 6) * 3 / 4) - 15), GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();

            if (Model && NewRootName.Length == 0)
            {
                NewRootName = Model.name;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New Pivot Location", EditorStyles.helpBox, GUILayout.Width(((position.width - 6) / 4)));
            CurrentPivotType = EditorGUILayout.Popup(CurrentPivotType, PivotTypes, GUILayout.Width(((position.width - 6) * 3 / 4) - 15), GUILayout.Height(20));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use New Scale", EditorStyles.helpBox, GUILayout.Width(((position.width - 6) / 4)));
            UseNewScale = EditorGUILayout.Toggle(UseNewScale, GUILayout.Width(((position.width - 6) * 4 / 4) - 15));
            EditorGUILayout.EndHorizontal();

            if (UseNewScale)
            {
                EditorGUILayout.Space();

                NewScale = EditorGUILayout.Vector3Field("New Scale", NewScale, GUILayout.Width(((position.width - 6) * 4 / 4) - 15), GUILayout.Height(20));
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Use New Rotation", EditorStyles.helpBox, GUILayout.Width(((position.width - 6) / 4)));
            UseNewRotation = EditorGUILayout.Toggle(UseNewRotation, GUILayout.Width(((position.width - 6) * 4 / 4) - 15));
            EditorGUILayout.EndHorizontal();

            if (UseNewRotation)
            {
                EditorGUILayout.Space();

                NewRotation = EditorGUILayout.Vector3Field("New Rotation", NewRotation, GUILayout.Width(((position.width - 6) * 4 / 4) - 15), GUILayout.Height(20));
            }

            GUILayout.EndArea();
        }

        private void CreateModel()
        {
            switch (CurrentPivotType)
            {
                case 0:
                    {
                        NewRoot = ModelUtils.CreateNewRoot(Model, PIVOT_TYPE.NO_CHANGE);
                    }
                    break;
                case 1:
                    {
                        NewRoot = ModelUtils.CreateNewRoot(Model, PIVOT_TYPE.CENTER);
                    }
                    break;
                case 2:
                    {
                        NewRoot = ModelUtils.CreateNewRoot(Model, PIVOT_TYPE.CENTER_TOP);
                    }
                    break;
                case 3:
                    {
                        NewRoot = ModelUtils.CreateNewRoot(Model, PIVOT_TYPE.CENTER_BOTTOM);
                    }
                    break;
                case 4:
                    {
                        NewRoot = ModelUtils.CreateNewRoot(Model, PIVOT_TYPE.CENTER_RIGHT);
                    }
                    break;
                case 5:
                    {
                        NewRoot = ModelUtils.CreateNewRoot(Model, PIVOT_TYPE.CENTER_LEFT);
                    }
                    break;
                case 6:
                    {
                        NewRoot = ModelUtils.CreateNewRoot(Model, PIVOT_TYPE.CENTER_FRONT);
                    }
                    break;
                case 7:
                    {
                        NewRoot = ModelUtils.CreateNewRoot(Model, PIVOT_TYPE.CENTER_BACK);
                    }
                    break;
                default:
                    break;
            }

            if (NewRootName.Length == 0)
            {
                NewRootName = Model.name;
            }
            else
            {
                NewRoot.name = NewRootName;
            }

            if (UseNewScale)
            {
                if (NewScale.sqrMagnitude == 0)
                {
                    NewScale = Vector3.one;
                }

                NewRoot.transform.GetChild(0).localScale = NewScale;
            }

            if (UseNewRotation)
            {
                NewRoot.transform.GetChild(0).localEulerAngles = NewRotation;
            }
        }
    }
}