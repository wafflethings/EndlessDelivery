using UnityEngine;

namespace EndlessDelivery.Gameplay
{
    public static class GenerationEquations
    {
        // https://www.desmos.com/calculator/5bot6npkk1
        public static int PresentAmount(int wave)
        {
            Debug.Log($"{wave} presents {6 + Mathf.CeilToInt(Mathf.Log(wave / 4f, 6.5f) * 4)}");
            return Mathf.Clamp(Mathf.CeilToInt(Mathf.Log(wave, 6.5f) * 4), 4, 12);
        }

        public static int[] DistributeBetween(int amount, int number)
        {
            int[] result = new int[amount];
            int index = 0;
            
            while (number >= 1)
            {
                result[index]++;
                number--;
                index++;

                if (index > amount - 1)
                {
                    index = 0;
                }
            }

            return result;
        } 
    }
}