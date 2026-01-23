using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public Text instructionText; // Текстовое поле для отображения инструкций
    public string[] instructions; // Массив строк с инструкциями
    private int currentInstructionIndex; // Индекс текущей инструкции

    void Start()
    {
        ShowInstruction(); // Показать первую инструкцию при запуске сцены
    }

    public void ShowNextInstruction()
    {
        currentInstructionIndex++; // Перейти к следующей инструкции
        if (currentInstructionIndex >= instructions.Length)
        {
            // Если все инструкции пройдены, завершить обучающий уровень
            Debug.Log("Tutorial completed!");
            return;
        }
        ShowInstruction(); // Показать следующую инструкцию
    }

    private void ShowInstruction()
    {
        instructionText.text = instructions[currentInstructionIndex]; // Отобразить текущую инструкцию
    }
}