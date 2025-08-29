using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Project/Readme", fileName = "Readme", order = 0)]
public class Readme : ScriptableObject
{
    public Texture2D Icon;
    public string Title;
    public Section[] Sections;
    public bool LoadedLayout;

    [Serializable]
    public class Section
    {
        public string Heading;
        [TextArea(10, 100)]
        public string Text;
        public string[] LinkText;
        public string[] Url;
    }
}
