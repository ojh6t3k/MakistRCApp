using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Ardunity
{
	[AddComponentMenu("ARDUnity/Utility/UI/ListItem")]
    [HelpURL("https://sites.google.com/site/ardunitydoc/references/utility/listitem")]
    [RequireComponent(typeof(Toggle))]
    public class ListItem : MonoBehaviour, IPointerClickHandler
    {
        public ListView owner;
        public Image image;
        [SerializeField]
        public Text[] textList;
        public System.Object data;

        private Toggle _toggle;

        public int index
        {
            get
            {
                return this.transform.GetSiblingIndex();
            }
        }

        // This property is for ListView
        public bool selected
        {
            get
            {
                if (_toggle == null)
                    _toggle = GetComponent<Toggle>();

                return _toggle.isOn;
            }
            set
            {
                if (_toggle == null)
                    _toggle = GetComponent<Toggle>();

                _toggle.isOn = value;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            owner.selectedItem = this;
        }
    }
}
