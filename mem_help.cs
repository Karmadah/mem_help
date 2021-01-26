using Memory.utils;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryno.bit32
{
    class mem_help32
    {
        Process process;
        public mem_help32(string target)
        {
            process = Process.GetProcessesByName(target)[0];
            if (process == null)
                return;
        }

        public uint get_base_address(uint start_address)
        {
            return (uint)process.MainModule.BaseAddress + start_address;
        }

        public byte[] read_mem_bytes(uint mem_address, uint bytes)
        {
            byte[] data = new byte[bytes];
            ReadProcessMemory(process.Handle, mem_address, data, data.Length, IntPtr.Zero);
            return data;
        }

        public T read_mem<T>(uint mem_address)
        {
            byte[] data = read_mem_bytes(mem_address, (uint)Marshal.SizeOf(typeof(T)));

            T t;
            GCHandle pinned_struct = GCHandle.Alloc(data, GCHandleType.Pinned);
            try { t = (T)Marshal.PtrToStructure(pinned_struct.AddrOfPinnedObject(), typeof(T)); }
            catch (Exception ex) { throw ex; }
            finally { pinned_struct.Free(); }

            return t;
        }

        public bool write_mem<T>(uint mem_address, T value)
        {
            IntPtr bw = IntPtr.Zero;

            int sz = object_type.get_size<T>();
            byte[] data = object_type.get_bytes<T>(value);
            bool result = WriteProcessMemory(process.Handle, mem_address, data, sz, out bw);
            return result && bw != IntPtr.Zero;
        }

        public void close()
        {
            CloseHandle(process.Handle);
        }

        #region PInvoke
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            uint lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            uint lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten
            );

        [DllImport("kernel32.dll")]
        private static extern Int32 CloseHandle(IntPtr hProcess);
        #endregion
    }
}

namespace Ryno.bit64
{
    class mem_help64
    {
        Process process;

        public mem_help64(string target)
        {
            process = Process.GetProcessesByName(target)[0];
            if (process == null)
                return;
        }

        public ulong get_base_address(ulong start_address)
        {
            return (ulong)process.MainModule.BaseAddress.ToInt64() + start_address;
        }

        public byte[] read_mem_bytes(ulong mem_address, int bytes)
        {
            byte[] data = new byte[bytes];
            ReadProcessMemory(process.Handle, mem_address, data, data.Length, IntPtr.Zero);
            return data;
        }

        public T read_mem<T>(ulong mem_address)
        {
            byte[] data = read_mem_bytes(mem_address, Marshal.SizeOf(typeof(T)));

            T t;
            GCHandle pinned_struct = GCHandle.Alloc(data, GCHandleType.Pinned);
            try { t = (T)Marshal.PtrToStructure(pinned_struct.AddrOfPinnedObject(), typeof(T)); }
            catch (Exception ex) { throw ex; }
            finally { pinned_struct.Free(); }

            return t;
        }

        public bool write_mem<T>(ulong mem_address, T value)
        {
            IntPtr bw = IntPtr.Zero;

            int sz = object_type.get_size<T>();
            byte[] data = object_type.get_bytes<T>(value);
            bool result = WriteProcessMemory(process.Handle, mem_address, data, sz, out bw);
            return result && bw != IntPtr.Zero;
        }

        public void close()
        {
            CloseHandle(process.Handle);
        }

        #region PInvoke
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            ulong lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(
            IntPtr hProcess,
            ulong lpBaseAddress,
            byte[] lpBuffer,
            int nSize,
            out IntPtr lpNumberOfBytesWritten
            );

        [DllImport("kernel32.dll")]
        private static extern Int32 CloseHandle(IntPtr hProcess);
        #endregion
    }
}

namespace Ryno.utils
{
    static class mem_utils
    {
        public static uint offset_calculator(bit32.mem_help32 target_mem, uint base_address, int[] offsets)
        {
            var address = base_address;
            foreach (uint offset in offsets)
            {
                address = target_mem.read_mem<uint>(address) + offset;
            }
            return address;
        }

        public static ulong offset_calculator(bit64.mem_help64 target_mem, ulong base_address, int[] offsets)
        {
            var address = base_address;
            foreach (uint offset in offsets)
            {
                address = target_mem.read_mem<ulong>(address) + offset;
            }
            return address;
        }
    }

    static class math
    {
        public static double[] make_vec(int number_of_values, double value = 0.0)
        {
            double[] result = new double[number_of_values];
            for (int q = 0; q < number_of_values; q++)
                result[i] = value;
            return result;
        }

        public static double[] multiply_vec2(double[] first_vec, double[] second_vec)
        {
            double[] result = new double[2];
            for (int q = 0; q < 2; q++)
                result[q] = first_vec[q] * second_vec[q];
            return result;
        }

        public static double[] multiply_vec3(double[] first_vec, double[] second_vec, double[] third_vec)
        {
            double[] result = new double[3];
            for (int q = 0; q < 3; q++)
                result[q] = first_vec[q] * second_vec[q] * third_vec[q];
            return result;
        }

        public static double[] add_vec2(double[] first_vec, double[] second_vec)
        {
            double[] result = new double[2];
            for (int q = 0; q < 2; q++)
                result[q] = first_vec[q] + second_vec[q];
            return result;
        }

        public static double[] add_vec3(double[] first_vec, double[] second_vec, double[] third_vec)
        {
            double[] result = new double[3];
            for (int q = 0; q < 3; q++)
                result[q] = first_vec[q] + second_vec[q] + third_vec[q];
            return result;
        }
    }

    public static class object_type
    {
        public static int get_size<T>()
        {
            return Marshal.SizeOf(typeof(T));
        }

        public static byte[] get_bytes<T>(T value)
        {
            string typename = typeof(T).ToString();
            Console.WriteLine(typename);
            switch (typename)
            {
                case "System.Single":
                    return BitConverter.GetBytes((float)Convert.ChangeType(value, typeof(float)));
                case "System.Int32":
                    return BitConverter.GetBytes((int)Convert.ChangeType(value, typeof(int)));
                case "System.Int64":
                    return BitConverter.GetBytes((long)Convert.ChangeType(value, typeof(long)));
                case "System.Double":
                    return BitConverter.GetBytes((double)Convert.ChangeType(value, typeof(double)));
                case "System.Byte":
                    return BitConverter.GetBytes((byte)Convert.ChangeType(value, typeof(byte)));
                case "System.String":
                    return Encoding.Unicode.GetBytes((string)Convert.ChangeType(value, typeof(string)));
                default:
                    return new byte[0];
            }
        }
    }
}