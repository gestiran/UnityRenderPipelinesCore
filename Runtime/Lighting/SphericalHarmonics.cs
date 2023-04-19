using System;

namespace UnityEngine.Rendering
{
    /// <summary>
    /// Structure holding Spherical Harmonic L1 coefficient.
    /// </summary>
    [Serializable]
    public struct SphericalHarmonicsL1
    {
        /// <summary>
        /// Red channel of each of the three L1 SH coefficient.
        /// </summary>
        public Vector4 shAr;
        /// <summary>
        /// Green channel of each of the three L1 SH coefficient.
        /// </summary>
        public Vector4 shAg;
        /// <summary>
        /// Blue channel of each of the three L1 SH coefficient.
        /// </summary>
        public Vector4 shAb;

        /// <summary>
        /// A set of L1 coefficients initialized to zero.
        /// </summary>
        public static readonly SphericalHarmonicsL1 zero = new SphericalHarmonicsL1
        {
            shAr = Vector4.zero,
            shAg = Vector4.zero,
            shAb = Vector4.zero
        };

        // These operators are implemented so that SphericalHarmonicsL1 matches API of SphericalHarmonicsL2.

        /// <summary>
        /// Sum two SphericalHarmonicsL1.
        /// </summary>
        /// <param name="lhs">First SphericalHarmonicsL1.</param>
        /// <param name="rhs">Second SphericalHarmonicsL1.</param>
        /// <returns>The resulting SphericalHarmonicsL1.</returns>
        public static SphericalHarmonicsL1 operator +(SphericalHarmonicsL1 lhs, SphericalHarmonicsL1 rhs) => new SphericalHarmonicsL1()
        {
            shAr = lhs.shAr + rhs.shAr,
            shAg = lhs.shAg + rhs.shAg,
            shAb = lhs.shAb + rhs.shAb
        };

        /// <summary>
        /// Subtract two SphericalHarmonicsL1.
        /// </summary>
        /// <param name="lhs">First SphericalHarmonicsL1.</param>
        /// <param name="rhs">Second SphericalHarmonicsL1.</param>
        /// <returns>The resulting SphericalHarmonicsL1.</returns>
        public static SphericalHarmonicsL1 operator -(SphericalHarmonicsL1 lhs, SphericalHarmonicsL1 rhs) => new SphericalHarmonicsL1()
        {
            shAr = lhs.shAr - rhs.shAr,
            shAg = lhs.shAg - rhs.shAg,
            shAb = lhs.shAb - rhs.shAb
        };

        /// <summary>
        /// Multiply two SphericalHarmonicsL1.
        /// </summary>
        /// <param name="lhs">First SphericalHarmonicsL1.</param>
        /// <param name="rhs">Second SphericalHarmonicsL1.</param>
        /// <returns>The resulting SphericalHarmonicsL1.</returns>
        public static SphericalHarmonicsL1 operator *(SphericalHarmonicsL1 lhs, float rhs) => new SphericalHarmonicsL1()
        {
            shAr = lhs.shAr * rhs,
            shAg = lhs.shAg * rhs,
            shAb = lhs.shAb * rhs
        };

        /// <summary>
        /// Divide two SphericalHarmonicsL1.
        /// </summary>
        /// <param name="lhs">First SphericalHarmonicsL1.</param>
        /// <param name="rhs">Second SphericalHarmonicsL1.</param>
        /// <returns>The resulting SphericalHarmonicsL1.</returns>
        public static SphericalHarmonicsL1 operator /(SphericalHarmonicsL1 lhs, float rhs) => new SphericalHarmonicsL1()
        {
            shAr = lhs.shAr / rhs,
            shAg = lhs.shAg / rhs,
            shAb = lhs.shAb / rhs
        };

        /// <summary>
        /// Compare two SphericalHarmonicsL1.
        /// </summary>
        /// <param name="lhs">First SphericalHarmonicsL1.</param>
        /// <param name="rhs">Second SphericalHarmonicsL1.</param>
        /// <returns>Whether the SphericalHarmonicsL1 match.</returns>
        public static bool operator ==(SphericalHarmonicsL1 lhs, SphericalHarmonicsL1 rhs)
        {
            return lhs.shAr == rhs.shAr
                && lhs.shAg == rhs.shAg
                && lhs.shAb == rhs.shAb;
        }

        /// <summary>
        /// Check two SphericalHarmonicsL1 inequality.
        /// </summary>
        /// <param name="lhs">First SphericalHarmonicsL1.</param>
        /// <param name="rhs">Second SphericalHarmonicsL1.</param>
        /// <returns>Whether the SphericalHarmonicsL1 are different.</returns>
        public static bool operator !=(SphericalHarmonicsL1 lhs, SphericalHarmonicsL1 rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compare this SphericalHarmonicsL1 with an object.
        /// </summary>
        /// <param name="other">The object to compare with.</param>
        /// <returns>Whether the SphericalHarmonicsL1 is equal to the object passed.</returns>
        public override bool Equals(object other)
        {
            if (!(other is SphericalHarmonicsL1)) return false;
            return this == (SphericalHarmonicsL1)other;
        }

        /// <summary>
        /// Produces an hash code of the SphericalHarmonicsL1.
        /// </summary>
        /// <returns>The hash code for this SphericalHarmonicsL1.</returns>
        public override int GetHashCode()
        {
            return ((17 * 23 + shAr.GetHashCode()) * 23 + shAg.GetHashCode()) * 23 + shAb.GetHashCode();
        }
    }

    /// <summary>
    /// A collection of utility functions used to access and set SphericalHarmonicsL2 in a more verbose way.
    /// </summary>
    public class SphericalHarmonicsL2Utils
    {
        
        public static void SetL0(ref SphericalHarmonicsL2 sh, Vector3 L0)
        {
            sh[0, 0] = L0.x;
            sh[1, 0] = L0.y;
            sh[2, 0] = L0.z;
        }

        /// <summary>
        /// Set the red channel for each of the L1 coefficients.
        /// </summary>
        /// <param name ="sh">The SphericalHarmonicsL2 data structure to store information on.</param>
        /// <param name ="L1_R">The red channels for each L1 coefficient.</param>
        public static void SetL1R(ref SphericalHarmonicsL2 sh, Vector3 L1_R)
        {
            sh[0, 1] = L1_R.x;
            sh[0, 2] = L1_R.y;
            sh[0, 3] = L1_R.z;
        }

        public static void SetL1G(ref SphericalHarmonicsL2 sh, Vector3 L1_G)
        {
            sh[1, 1] = L1_G.x;
            sh[1, 2] = L1_G.y;
            sh[1, 3] = L1_G.z;
        }

        public static void SetL1B(ref SphericalHarmonicsL2 sh, Vector3 L1_B)
        {
            sh[2, 1] = L1_B.x;
            sh[2, 2] = L1_B.y;
            sh[2, 3] = L1_B.z;
        }

        public static void SetCoefficient(ref SphericalHarmonicsL2 sh, int index, Vector3 coefficient)
        {
            sh[0, index] = coefficient.x;
            sh[1, index] = coefficient.y;
            sh[2, index] = coefficient.z;
        }
    }
}
