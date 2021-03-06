﻿namespace ITCompanySimulation.Utilities
{
    public class Utils
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        /// <summary>
        /// Maps one range of values to another range of values
        /// </summary>
        /// <param name="input">Value that will be mapped to another range</param>
        /// <param name="inputMin">Minimum value of input range</param>
        /// <param name="inputMax">Maximum value of input range</param>
        /// <param name="outputMin">Minimum value of output range</param>
        /// <param name="outputMax">Maximum value of output range</param>
        /// <returns></returns>
        public static float MapRange(float input, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            return outputMin + ((outputMax - outputMin) / (inputMax - inputMin)) * (input - inputMin);
        }
    } 
}
