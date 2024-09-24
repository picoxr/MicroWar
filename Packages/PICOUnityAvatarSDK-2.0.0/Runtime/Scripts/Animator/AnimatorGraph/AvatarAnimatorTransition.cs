using UnityEngine;
using Pico.Avatar.XNode;

namespace Pico
{
	namespace Avatar
	{
		[System.Serializable]
		public class Transition
		{
			[Tooltip(
				"If selected, this transition will automatically be triggered when animation end. Only one transition is allowed to be selected.")]
			public bool autoSwitch;

			[Delayed] public float duration;
			public AvatarConditionType conditionType;
			public StringDropDown parameter;
			public BoolCompare boolOperator = BoolCompare.True;
			public UintCompare uintOperator = UintCompare.Greater;
			public FloatCompare floatOperator = FloatCompare.Greater;
			[Delayed] public uint comparedUint = 0;
			[Delayed] public float comparedFloat = 0;

			public void UpdateParameters(Node state)
			{
				if (parameter == null)
				{
					parameter = new StringDropDown("Parameter");
				}

				if (state == null)
				{
					Debug.LogError("Transition : state is null");
				}

				if (conditionType == AvatarConditionType.Bool)
				{
					parameter.stringArray = state.layer.parentAnimator.BoolParametersArray;
				}
				else if (conditionType == AvatarConditionType.Trigger)
				{
					parameter.stringArray = state.layer.parentAnimator.TriggerParametersArray;
				}
				else if (conditionType == AvatarConditionType.Uint)
				{
					parameter.stringArray = state.layer.parentAnimator.UintParametersArray;
				}
				else if (conditionType == AvatarConditionType.Float)
				{
					parameter.stringArray = state.layer.parentAnimator.FloatParametersArray;
				}
			}
		}

		//Inheriting Condition is because it is necessary to 
		//create a node when dragging Output to a blank place
		[System.Serializable]
		public class Enter : Transition
		{
		}
	}
}