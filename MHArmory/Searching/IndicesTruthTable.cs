using System;

namespace MHArmory.Searching
{
    public class IndicesTruthTable
    {
        private int currentValue;
        private int currentBitsValue;
        private int[] indices;
        private readonly int maxBitsValue;

        public int IndicesCount => indices.Length;

        public IndicesTruthTable(int indicesCount)
        {
            if (indicesCount <= 0 || indicesCount > 32)
                throw new ArgumentException($"Invalid '{nameof(indicesCount)}' argument. Must be comprised between 1 and 32 included.", nameof(indicesCount));

            indices = new int[indicesCount];
            maxBitsValue = (int)Math.Pow(2, indices.Length);
        }

        public void Next(int[] output)
        {
            if (output == null)
                throw new ArgumentNullException(nameof(output));
            if (output.Length != indices.Length)
                throw new ArgumentException($"Invalid '{nameof(output)}' argument. Must be of size {indices.Length} but is of size {output.Length}.", nameof(output));

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
