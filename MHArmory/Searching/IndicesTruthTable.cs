using System;

namespace MHArmory.Searching
{
    public class IndicesTruthTable
    {
        private int currentValue;
        private int currentBitsValue;
        private readonly int[] indices = new int[5];
        private readonly int maxBitsValue;

        public IndicesTruthTable()
        {
            maxBitsValue = (int)Math.Pow(2, indices.Length);
        }

        public void Next(int[] output)
        {
            if (output == null || output.Length != indices.Length)
                throw new ArgumentException(nameof(output));

            for (int i = 0; i < indices.Length; i++)
                output[i] = indices[i];

            currentBitsValue = (currentBitsValue + 1) % maxBitsValue;
            if (currentBitsValue == 0)
            {
                currentValue++;
                currentBitsValue = 1;
            }

            int maxIndex = indices.Length - 1;
            for (int i = 0; i < indices.Length; i++)
            {
                int bitValue = (currentBitsValue >> (maxIndex - i) & 1);
                indices[i] = currentValue + bitValue;
            }
        }
    }
}
