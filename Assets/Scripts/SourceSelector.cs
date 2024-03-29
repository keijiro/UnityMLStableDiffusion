using Klak.TestTools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

public sealed class SourceSelector : MonoBehaviour
{
    #region Editable properties

    [field:SerializeField]
    public Texture2D[] ImageList = null;

    #endregion

    #region Accessors for UI Toolkit

    [CreateProperty]
    public List<string> ImageNameList
      => ImageList.Select(x => x.name).ToList();

    [CreateProperty]
    public List<string> WebcamNameList
      => WebCamTexture.devices.Select(x => x.name).ToList();

    void OnChangeTab(Tab prev, Tab next)
    {
        ImageDropdown.value = null;
        WebcamDropdown.value = null;
    }

    void OnSelectImage(ChangeEvent<string> evt)
    {
        if (string.IsNullOrEmpty(evt.newValue)) return;
        Target.SourceType = ImageSourceType.Texture;
        Target.SourceTexture = ImageList.First(x => x.name == evt.newValue);
        ImagePreview.style.backgroundImage
          = Background.FromRenderTexture(Target.AsRenderTexture);
    }

    void OnSelectWebcam(ChangeEvent<string> evt)
    {
        if (string.IsNullOrEmpty(evt.newValue)) return;
        Target.SourceType = ImageSourceType.Webcam;
        Target.SourceName = evt.newValue;
        WebcamPreview.style.backgroundImage
          = Background.FromRenderTexture(Target.AsRenderTexture);
    }

    #endregion

    #region Private helper properties

    ImageSource Target
      => GetComponent<ImageSource>();

    VisualElement UIRoot
      => GetComponent<UIDocument>().rootVisualElement;

    TabView SourceTabs
      => UIRoot.Q<TabView>("image-source-tabs");

    DropdownField ImageDropdown
      => UIRoot.Q<DropdownField>("image-selector");

    VisualElement ImagePreview
      => UIRoot.Q<VisualElement>("image-preview");

    DropdownField WebcamDropdown
      => UIRoot.Q<DropdownField>("webcam-selector");

    VisualElement WebcamPreview
      => UIRoot.Q<VisualElement>("webcam-preview");

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        SourceTabs.activeTabChanged += OnChangeTab;

        ImageDropdown.dataSource = this;
        ImageDropdown.RegisterValueChangedCallback(OnSelectImage);

        WebcamDropdown.dataSource = this;
        WebcamDropdown.RegisterValueChangedCallback(OnSelectWebcam);
    }

    #endregion
}
