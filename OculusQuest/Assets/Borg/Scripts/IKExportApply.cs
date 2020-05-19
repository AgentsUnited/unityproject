using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class IKExportApply : MonoBehaviour {

	public FullBodyBipedIK fbbik;

	public void Apply() {
		if (fbbik != null) fbbik.UpdateSolverExternal();
	}
}
