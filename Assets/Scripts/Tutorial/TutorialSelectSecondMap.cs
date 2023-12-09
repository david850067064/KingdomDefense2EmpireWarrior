using Data;
using UnityEngine;

namespace Tutorial
{
	public class TutorialSelectSecondMap : TutorialUnit
	{
		private string TutorialID = ReadWriteDataTutorial.TUTORIAL_ID_SELECT_SECOND_MAP;

		protected override void SaveTutorialPassed()
		{
			UnityEngine.Debug.Log("Click vào chọn map, hoàn thành bước chọn map!");
			ReadWriteDataTutorial.Instance.SetTutorialStatus(TutorialID, value: true);
		}

		protected override bool ShouldShowTutorial()
		{
			return !ReadWriteDataTutorial.Instance.GetTutorialStatus(TutorialID);
		}
	}
}
