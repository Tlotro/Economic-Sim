
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Mosframe
{
	public class DynamicScrollViewResource : UIBehaviour, IDynamicScrollViewItem
	{
		public Controller controller;
		public TMPro.TMP_Text ResNameBox;
        public Mosframe.DynamicVScrollView scrollView;

        public void Remove()
		{
			controller.Resources.RemoveAt(controller.Resources.FindIndex(x=>x.name == ResNameBox.text));
            scrollView.totalItemCount--;
			Controller.instance.CheckButton();
        }

		public void onUpdateItem(int index)
		{
			ResNameBox.text = controller.Resources[index].name;
		}

		public void RemoveItem()
		{

		}
	}
}
