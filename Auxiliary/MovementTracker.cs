using UnityEngine;
using UnityEngine.InputSystem;
namespace DroneController
{
    public class MovementTracker : MonoBehaviour
    {
        public TutorialManager tutorialManager; // Менеджер обучения
        public float requiredMovementTime = 3f; // Требуемое время движения
        public GameObject _CM;
        public GameObject _timer;
        public CheckpointManager _CMM;
        private float movementTimer; // Таймер движения
        private int instructionCount;

        private bool checkpointman = true;


        private void Start()
        {
            instructionCount = 0;
            
        }

        private void Update()
        {
            if (instructionCount == 1) {
                movementTimer += Time.deltaTime;
                if (movementTimer >= requiredMovementTime) // Если таймер достиг требуемого времени
                {
                    tutorialManager.ShowNextInstruction(); // Вызвать метод отображения следующей инструкции из TutorialManager
                    movementTimer = 0f; // Сбросить таймер движения
                    instructionCount += 1;
                }
                //WASDchecker();
            }
            if (instructionCount == 0) {
                movementTimer += Time.deltaTime;
                if (movementTimer >= requiredMovementTime) // Если таймер достиг требуемого времени
                {
                    tutorialManager.ShowNextInstruction(); // Вызвать метод отображения следующей инструкции из TutorialManager
                    movementTimer = 0f; // Сбросить таймер движения
                    instructionCount += 1;

                }
                //IJKLchecker();
            }
            if (instructionCount == 2 && checkpointman == true) {
                if (checkpointman){
                _timer.SetActive(true);
                _CM.SetActive(true);
                checkpointman = false;
                instructionCount += 1;
                checkpointman = true;
                }
            }
            if (_CMM._finish == true && checkpointman == true)
            {
                //tutorialManager.ShowNextInstruction();
                _CM.SetActive(false);
                instructionCount += 1;
                Debug.Log(instructionCount);
                checkpointman = false;
                
            }
        }



        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag == "Respawn")
            {
            if (instructionCount == 4) {
                Landedchecker();
                }
            }
        }

        private void Landedchecker()
        {
            if (true) // Если время использования оси достигло требуемого значения
            {
                movementTimer += Time.deltaTime; // Увеличить таймер движения
                Debug.Log(movementTimer);
                if (movementTimer >= requiredMovementTime + 1) // Если таймер достиг требуемого времени
                {
                    tutorialManager.ShowNextInstruction(); // Вызвать метод отображения следующей инструкции из TutorialManager
                    movementTimer = 0f; // Сбросить таймер движения
                    instructionCount += 1;
                }
            }
        }
    }    
}