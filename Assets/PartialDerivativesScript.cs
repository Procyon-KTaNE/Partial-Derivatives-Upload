using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class PartialDerivativesScript : MonoBehaviour {

		public KMBombInfo bomb;
		public KMAudio audio;

		public KMSelectable[] digits;
		public KMSelectable buttonS;
		public KMSelectable buttonC;

		public Material[] ledOptions;
		public Renderer mainLed;
		public Renderer stage1Led;
		public Renderer stage2Led;
		public Renderer stage3Led;

		public AudioClip[] sounds;

		private int[] ledIndex = new int[3];

		string[] vars = new string[3] {"x", "y", "z"};
		string[] oppList = new string[3] {"\n     + ", "\n     - ", ""};
		private int[] coeff = new int[3];
		private int[,] exponents = new int[3,3];
		private int[] oppNums = new int[3];

		private string function = " > ";
		public TextMesh display;

		public TextMesh userInput;

		private bool red;
		private bool yellow;
		private bool blue;

		private int[,] variables = new int[3,3];

		private int[] stageAnswers = new int[3];

		private int stage;
		static int moduleIdCounter = 1;
		int moduleId;
		private bool moduleSolved;

		void Awake() {
				moduleId = moduleIdCounter++;

				foreach(KMSelectable digit in digits) {
						KMSelectable pressedDigit = digit;
						digit.OnInteract += delegate() { onDigitPress(pressedDigit); return false; };
				}

				buttonS.OnInteract += delegate() { onSubmitPress(buttonS); return false; };
				buttonC.OnInteract += delegate() { onClearPress(buttonC); return false; };
		}

		void Start() {
				userInput.text = "> ";

				GenerateFunction();
				stage1Led.material = ledOptions[6];
				stage2Led.material = ledOptions[7];
				stage3Led.material = ledOptions[7];

				PickLedColors();
				mainLed.material = ledOptions[ledIndex[0]];

				FindVariables();

				for(int i = 0; i < 3; i++) {
						stage = i + 1;
						Derive();
						Evaluate();
						Debug.LogFormat("[Partial Derivatives #{0}] Stage " + stage + " answer is " + stageAnswers[i], moduleId);
				}

				stage = 1;
		}

		void GenerateFunction() {
				coeff = new int[3];
				exponents = new int[3,3]; // x1 y1 z1 / x2 y2 z2 / x3 y3 z3
				oppNums = new int[3];

				for(int i = 0; i < 3; i++) {
						coeff[i] = UnityEngine.Random.Range(1,10);
				}

				for(int i = 0; i < 3; i++) {
						for(int j = 0; j < 3; j++) {
								exponents[i,j] = UnityEngine.Random.Range(0,6);
						}
				}
				for(int i = 0; i < 3; i++) {
						if(exponents[i,i] == 0) {
								exponents[i,i] = UnityEngine.Random.Range(1,6);
						}
				}

				for(int i = 0; i < 2; i++) {
						oppNums[i] = UnityEngine.Random.Range(0,2);
				}
				oppNums[2] = 2;

				WriteFunction();
				display.text = function;
		}

		String Convert(int i) {
				if(i == 1) {
						return "";
				} else if(i == 2) {
						return "²";
				} else if(i == 3) {
						return "³";
				} else if(i == 4) {
						return "⁴";
				} else if(i == 5) {
						return "⁵";
				} else {
						return "?";
				}
		}

		void WriteFunction() {
				function = "f(x) = ";
				for(int i = 0; i < 3; i++) {
						if(coeff[i] == 0) {
								function += "0";
								function += oppList[oppNums[i]];
								continue;
						}
						if(coeff[i] > 1) {
								function += coeff[i].ToString();
						}
						for(int j = 0; j < 3; j++) {
								if(exponents[i,j] != 0) {
										function += vars[j];
										if(exponents[i,j] > 1) {
												function += Convert(exponents[i,j]);
										}
								}
						}
						function += oppList[oppNums[i]];
				}

				Debug.LogFormat("[Partial Derivatives #{0}] Stage " + stage + ": Function is {1}", moduleId, function.Replace("\n     ", " "));
		}

		void PickLedColors() { // Blue Green Orange Purple Red Yellow
				for(int i = 0; i < 3; i++) {
						ledIndex[i] = UnityEngine.Random.Range(0,6);
				}
		}

		void FindVariables() {
				int sn1 = ConvertLetter(bomb.GetSerialNumber()[0]);
				int sn2 = ConvertLetter(bomb.GetSerialNumber()[1]);
				int sn3 = ConvertLetter(bomb.GetSerialNumber()[2]);
				int sn4 = ConvertLetter(bomb.GetSerialNumber()[3]);
				int sn5 = ConvertLetter(bomb.GetSerialNumber()[4]);
				int sn6 = ConvertLetter(bomb.GetSerialNumber()[5]);
				int b   = bomb.GetBatteryCount();
				int bh  = bomb.GetBatteryHolderCount();
				int li  = bomb.GetOnIndicators().Count();
				int ui  = bomb.GetOffIndicators().Count();
				int p   = bomb.GetPortCount();
				int pp  = bomb.GetPortPlates().Count();

				variables[0,0] = (sn1 + ui + 10) % 10; // x1
				variables[1,0] = (sn3 - pp + 10) % 10; // x2
				variables[2,0] = (ui  + b  + 10) % 10; // x3
				variables[0,1] = (sn5 - li + 10) % 10; // y1
				variables[1,1] = (b   + bh + 10) % 10; // y2
				variables[2,1] = (sn4 - p  + 10) % 10; // y3
				variables[0,2] = (bh  + pp + 10) % 10; // z1
				variables[1,2] = (sn6 - li + 10) % 10; // z2
				variables[2,2] = (sn2 + p  + 10) % 10; // z3

				for(int i = 0; i < 3; i++) {
						for(int j = 0; j < 3; j++) {
								if(variables[i,j] == 0) {
										variables[i,j] = 1;
								}
						}
				}

				Debug.LogFormat("[Partial Derivatives #{0}] Variable values\n"
											+ "Stage 1: x = {1}, y = {2}, z = {3}\n"
											+ "Stage 2: x = {4}, y = {5}, z = {6}\n"
											+ "Stage 3: x = {7}, y = {8}, z = {9}\n", moduleId, variables[0,0], variables[1,0], variables[2,0], variables[0,1], variables[1,1], variables[2,1], variables[0,2], variables[1,2], variables[2,2]);
		}

		int ConvertLetter(char c) {
				if(c > 64) {
						return c - 64;
				} else if(c > 47) {
						return c - 48;
				}
				return -1;
		}

		void Derive() {
				DetermineVariableToDeriveWRT();

				if((stage == 1 && red) || (stage == 2 && blue) || (stage == 3 && yellow)) {
						Debug.LogFormat("[Partial Derivatives #{0}] Stage " + stage + ": Color is {1}, Deriving with respect to x", moduleId, ledOptions[ledIndex[stage - 1]].name);
						Dx();
				} else if((stage == 1 && yellow) || (stage == 2 && red) || (stage == 3 && blue)) {
						Debug.LogFormat("[Partial Derivatives #{0}] Stage " + stage + ": Color is {1}, Deriving with respect to y", moduleId, ledOptions[ledIndex[stage - 1]].name);
						Dy();
				} else if((stage == 1 && blue) || (stage == 2 && yellow) || (stage == 3 && red)) {
						Debug.LogFormat("[Partial Derivatives #{0}] Stage " + stage + ": Color is {1}, Deriving with respect to z", moduleId, ledOptions[ledIndex[stage - 1]].name);
						Dz();
				}
		}

		void Dx() {
				for(int i = 0; i < 3; i++) {
						coeff[i] *= exponents[i,0];
						exponents[i,0]--;
				}

				WriteFunction();
		}

		void Dy() {
				for(int i = 0; i < 3; i++) {
						coeff[i] *= exponents[i,1];
						exponents[i,1]--;
				}

				WriteFunction();
		}

		void Dz() {
				for(int i = 0; i < 3; i++) {
						coeff[i] *= exponents[i,2];
						exponents[i,2]--;
				}

				WriteFunction();
		}

		void DetermineVariableToDeriveWRT() { // Blue Green Orange Purple Red Yellow
				if(ledIndex[stage - 1] == 4 || ledIndex[stage - 1] == 1) {
						red = true;
						yellow = false;
						blue = false;
				} else if(ledIndex[stage - 1] == 5 || ledIndex[stage - 1] == 3) {
						red = false;
						yellow = true;
						blue = false;
				} else if(ledIndex[stage - 1] == 0 || ledIndex[stage - 1] == 2) {
						red = false;
						yellow = false;
						blue = true;
				}
		}

		void Evaluate() {
				double answer = (
						(coeff[0] * Math.Pow(variables[stage - 1, 0], exponents[0,0])
											* Math.Pow(variables[stage - 1, 1], exponents[0,1])
											* Math.Pow(variables[stage - 1, 2], exponents[0,2]))
								 + (-2 * oppNums[0] + 1) *
						(coeff[1] * Math.Pow(variables[stage - 1, 0], exponents[1,0])
						 					* Math.Pow(variables[stage - 1, 1], exponents[1,1])
						 					* Math.Pow(variables[stage - 1, 2], exponents[1,2]))
								 + (-2 * oppNums[1] + 1) *
						(coeff[2] * Math.Pow(variables[stage - 1, 0], exponents[2,0])
						 					* Math.Pow(variables[stage - 1, 1], exponents[2,1])
						 					* Math.Pow(variables[stage - 1, 2], exponents[2,2])));
				stageAnswers[stage - 1] = (int)(Math.Abs(answer) % 10000);
		}

		void onDigitPress(KMSelectable digit) {
				audio.PlaySoundAtTransform(sounds[1].name, transform);
				if(userInput.text.Length < 6) {
						userInput.text += digit.GetComponentInChildren<TextMesh>().text;
				}
		}

		void onClearPress(KMSelectable button) {
				audio.PlaySoundAtTransform(sounds[1].name, transform);
				userInput.text = "> ";
		}

		void onSubmitPress(KMSelectable button) {
				button.AddInteractionPunch();
				button.AddInteractionPunch(0.5f);
				if(stage == 1) {
						Debug.LogFormat("[Partial Derivatives #{0}] Inputted " + userInput.text.Substring(2, userInput.text.Length - 2) + ", expected " + stageAnswers[0], moduleId);
						if(userInput.text.Substring(2, userInput.text.Length - 2).Equals(stageAnswers[0].ToString())) {
								audio.PlaySoundAtTransform(sounds[2].name, transform);
								Debug.LogFormat("[Partial Derivatives #{0}] Stage 1 passed!", moduleId);
								userInput.text = "> ";
								stage2Led.material = ledOptions[6];
								mainLed.material = ledOptions[ledIndex[1]];
								stage++;
						} else {
								Debug.LogFormat("[Partial Derivatives #{0}] Strike given!", moduleId);
								GetComponent<KMBombModule>().HandleStrike();
								userInput.text = "> ";
						}
				} else if(stage == 2) {
						Debug.LogFormat("[Partial Derivatives #{0}] Inputted " + userInput.text.Substring(2, userInput.text.Length - 2) + ", expected " + stageAnswers[1], moduleId);
						if(userInput.text.Substring(2, userInput.text.Length - 2).Equals(stageAnswers[1].ToString())) {
								audio.PlaySoundAtTransform(sounds[2].name, transform);
								Debug.LogFormat("[Partial Derivatives #{0}] Stage 2 passed!", moduleId);
								userInput.text = "> ";
								stage3Led.material = ledOptions[6];
								mainLed.material = ledOptions[ledIndex[2]];
								stage++;
						} else {
								Debug.LogFormat("[Partial Derivatives #{0}] Strike given!", moduleId);
								GetComponent<KMBombModule>().HandleStrike();
								userInput.text = "> ";
						}
				} else if(stage == 3) {
						Debug.LogFormat("[Partial Derivatives #{0}] Inputted " + userInput.text.Substring(2, userInput.text.Length - 2) + ", expected " + stageAnswers[2], moduleId);
						if(userInput.text.Substring(2, userInput.text.Length - 2).Equals(stageAnswers[2].ToString())) {
								audio.PlaySoundAtTransform(sounds[3].name, transform);
								Debug.LogFormat("[Partial Derivatives #{0}] Stage 3 passed!", moduleId);
								display.text = "Well done!\nThis module\nis solved!";
								mainLed.material = ledOptions[6];
								Debug.LogFormat("[Partial Derivatives #{0}] Module solved!", moduleId);
								GetComponent<KMBombModule>().HandlePass();
						} else {
								Debug.LogFormat("[Partial Derivatives #{0}] Strike given!", moduleId);
								GetComponent<KMBombModule>().HandleStrike();
								userInput.text = "> ";
						}
				}
		}
}
