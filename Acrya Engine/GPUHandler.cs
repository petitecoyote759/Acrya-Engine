using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.CPU;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BOIDSimulator
{
    internal class GPUHandler
    {
        public static Context context = Context.CreateDefault();
        public static Accelerator accelerator;
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public static void Main()
        {







            // <<GPU Selection>> //
            Device device;
            PriorityQueue<Device, float> deviceQueue = new PriorityQueue<Device, float>();
            foreach (Device tempDevice in context)
            {
                if (tempDevice.AcceleratorType == AcceleratorType.Cuda)
                {
                    deviceQueue.Enqueue(tempDevice, 0f);
                }
                else if (tempDevice.AcceleratorType == AcceleratorType.OpenCL)
                {
                    deviceQueue.Enqueue(tempDevice, 1f);
                }
                else
                {
                    deviceQueue.Enqueue(tempDevice, 2f);
                }
            }
            if (deviceQueue.Count == 0) { return; }
            device = deviceQueue.Dequeue();
            //device = context.GetPreferredDevice(false);


            // <<Accelerator Creation>> //
            accelerator = device.AcceleratorType switch
            {
                AcceleratorType.CPU => context.CreateCPUAccelerator(0),
                AcceleratorType.OpenCL => context.CreateCLAccelerator(0),
                AcceleratorType.Cuda => context.CreateCudaAccelerator(0),

                _ => throw new NoGPUFoundException() // something has gone horribly wrong, there should always be an accelerator
            };


            Console.WriteLine($"Initiating with Accelerator: \'{accelerator}\'.");

            int valuesLength = 10000000;
            float[] test = new float[valuesLength];
            for (int i = 0; i < valuesLength; i++)
            {
                test[i] = (i + 1) / 2f;
            }


            loadedKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<float>, ArrayView<float>>(Kernel);


            Stopwatch stopwatch = Stopwatch.StartNew();
            float[] output = CalculateWithGPU(test);
            stopwatch.Stop();
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"{test[i]} -> {output[i]}");
            }

            Console.WriteLine($"GPU took {stopwatch.ElapsedTicks / (double)Stopwatch.Frequency} with subtime {time}\n");


            stopwatch = Stopwatch.StartNew();
            output = CalculateWithCPU(test);
            stopwatch.Stop();
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"{test[i]} -> {output[i]}");
            }

            Console.WriteLine($"CPU took {stopwatch.ElapsedTicks / (double)Stopwatch.Frequency}");












            // <<Disposal>> //
            accelerator.Dispose();
            context.Dispose();
        }

        private static void Kernel(Index1D i, ArrayView<float> data, ArrayView<float> output)
        {
            output[i] = MathFunction(data[i]);
        }
        static Action<Index1D, ArrayView<float>, ArrayView<float>> loadedKernel;
        private static float[] CalculateWithGPU(float[] test)
        {
            MemoryBuffer1D<float, Stride1D.Dense> gpuInts = accelerator.Allocate1D<float>(test);
            MemoryBuffer1D<float, Stride1D.Dense> outputGpuInts = accelerator.Allocate1D<float>(test.Length);
            //gpuInts.CopyFromCPU(test);

            


            Stopwatch stopwatch = Stopwatch.StartNew();
            loadedKernel((int)outputGpuInts.Length, gpuInts.View, outputGpuInts.View);

            accelerator.Synchronize();
            stopwatch.Stop();
            time = stopwatch.ElapsedTicks / (double)Stopwatch.Frequency;

            float[] output = outputGpuInts.GetAsArray1D();

            return output;
        }
        private static double time;




        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static float[] CalculateWithCPU(float[] test)
        {
            float[] output = new float[test.Length];
            for (int i = 0; i < test.Length; i++)
            {
                output[i] = MathFunction(test[i]);
            }
            return output;
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float MathFunction(float value)
        {
            return MathF.Sqrt(value) * value * value / 17f;
        }




        private static string GetInfoString(Accelerator a)
        {
            StringWriter infoString = new StringWriter();
            a.PrintInformation(infoString);
            return infoString.ToString();
        }
    }






    public class NoGPUFoundException : Exception
    {
        public NoGPUFoundException() : base($"No GPU was found and so the program cannot continue.") { }
    }
}
