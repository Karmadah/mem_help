using Ryno.utils;
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

        public static uint add_mem(bit32.mem_help32 target_mem, uint first_hex, uint second_hex)
        {
            uint address = target_mem.read_mem<uint>(first_hex) + second_hex;
            return address;
        }

        public static ulong add_mem(bit64.mem_help64 target_mem, ulong first_hex, ulong second_hex)
        {
            ulong address = target_mem.read_mem<ulong>(first_hex) + second_hex;
            return address;
        }

        public static uint sub_mem(bit32.mem_help32 target_mem, uint first_hex, uint second_hex)
        {
            uint address = target_mem.read_mem<uint>(first_hex) - second_hex;
            return address;
        }

        public static ulong sub_mem(bit64.mem_help64 target_mem, ulong first_hex, ulong second_hex)
        {
            ulong address = target_mem.read_mem<ulong>(first_hex) - second_hex;
            return address;
        }
    }

    class vec2
    {
        public float x;
        public float y;

        public vec2() { }

        public vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float distance(vec2 vector)
        {
            float dx = vector.x - x;
            float dy = vector.y - y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Math.Round(x, 2), Math.Round(y, 2));
        }

        public static vec2 multiply_vec2(vec2 first_vec, vec2 second_vec)
        {
            vec2 result = new vec2();
            result.x = first_vec.x * second_vec.x;
            result.y = first_vec.y * second_vec.y;
            return result;
        }

        public static vec2 add_vec2(vec2 first_vec, vec2 second_vec)
        {
            vec2 result = new vec2();
            result.x = first_vec.x + second_vec.x;
            result.y = first_vec.y + second_vec.y;
            return result;
        }
    }

    public class vec3
    {
        public float x;
        public float y;
        public float z;

        public vec3() { }

        public vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float distance(vec3 vector)
        {
            float dx = vector.x - x;
            float dy = vector.y - y;
            float dz = vector.z - z;
            return (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", Math.Round(x, 2), Math.Round(y, 2), Math.Round(z, 2));
        }

        public static vec3 multiply_vec3(vec3 first_vec, vec3 second_vec)
        {
            vec3 result = new vec3();
            result.x = first_vec.x * second_vec.x;
            result.y = first_vec.y * second_vec.y;
            result.z = first_vec.z * second_vec.z;
            return result;
        }

        public static vec3 add_vec3(vec3 first_vec, vec3 second_vec)
        {
            vec3 result = new vec3();
            result.x = first_vec.x + second_vec.x;
            result.y = first_vec.y + second_vec.y;
            result.z = first_vec.z + second_vec.z;
            return result;
        }
    }

    public class vec4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public vec4() { }

        public vec4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}, {3}", x, y, z, w);
        }
    }

    static class math
    {
        public static float pi = 3.14159265358979323846f;

        public static vec2 angle_calculator(vec3 vec_start, vec3 vec_final)
        {
            vec2 angle = new vec2();

            //calculate horizontal angle between enemy and player (yaw)
            float dx = vec_final.x - vec_start.x;
            float dy = vec_final.y - vec_start.y;
            double angle_yaw = Math.Atan2(dy, dx) * 180 / pi;

            //calculate verticle angle between enemy and player (pitch)
            double distance = Math.Sqrt(dx * dx + dy * dy);
            float dz = vec_final.z - vec_start.z;
            double angle_pitch = Math.Atan2(dz, distance) * 180 / pi;

            //set self angles to calculated angles
            angle.x = (float)angle_yaw + 90;
            angle.y = (float)angle_pitch;

            return angle;
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