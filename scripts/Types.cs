using Godot;

namespace CaveGen.Types
{
    // redefining vector structs bc I can't be bothered to switch between uppercase/lowercase
    struct Vec3
    {
        public float x;
        public float y;
        public float z;

        public static Vec3 ZERO => new(0, 0, 0);

        public Vec3(float _x, float _y, float _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public Vec3(Vector3 vec)
        {
            x = vec.X;
            y = vec.Y;
            z = vec.Z;
        }

        public Vec3(Vec3i vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        public static Vec3 operator +(Vec3 a, Vec3 b)
        {
            return new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vec3 operator *(Vec3 a, float b)
        {
            return new Vec3(a.x * b, a.y * b, a.z * b);
        }
    }

    struct Vec3i
    {
        public int x;
        public int y;
        public int z;

        public Vec3i(int _x, int _y, int _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }

        public Vec3i(Vector3I vec)
        {
            x = vec.X;
            y = vec.Y;
            z = vec.Z;
        }

        public static Vec3i operator +(Vec3i a, Vec3i b)
        {
            return new Vec3i(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vec3 operator *(Vec3i a, float b)
        {
            return new Vec3(a.x * b, a.y * b, a.z * b);
        }
    }
}
