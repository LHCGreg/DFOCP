using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dfo.Login
{
	// Why is this not in the framework? :(
	/// <summary>
	/// Struct holding two things that don't have to be the same type.
	/// </summary>
	/// <typeparam name="FirstType">Type of thing one.</typeparam>
	/// <typeparam name="SecondType">Type of thing two.</typeparam>
	public struct Pair<FirstType, SecondType>
	{
		private readonly FirstType m_first;

		/// <summary>
		/// Gets the first value of the pair.
		/// </summary>
		public FirstType First { get { return m_first; } }

		private readonly SecondType m_second;

		/// <summary>
		/// Gets the second value of the pair.
		/// </summary>
		public SecondType Second { get { return m_second; } }

		/// <summary>
		/// Creates a pair with the given values.
		/// </summary>
		/// <param name="first">The first value of the pair.</param>
		/// <param name="second">The second value of the pair.</param>
		public Pair( FirstType first, SecondType second )
		{
			m_first = first;
			m_second = second;
		}

		/// <summary>
		/// Compares this <c>Pair&lt;<typeparamref name="FirstType"/>, <typeparamref name="SecondType"/>&gt;</c>
		/// with another object for equality.
		/// </summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>True if <paramref name="obj"/> is not null and is a
		/// <c>Pair&lt;<typeparamref name="FirstType"/>, <typeparamref name="SecondType"/>&gt;</c> and the
		/// pairs are equal. They are considered to be equal if the first elements are equal and the second
		/// elements are equal.</returns>
		public override bool Equals( object obj )
		{
			if ( obj == null )
			{
				return false;
			}

			return obj is Pair<FirstType, SecondType> && Equals( (Pair<FirstType, SecondType>)obj );
		}

		/// <summary>
		/// Compares two pairs for value equality.
		/// </summary>
		/// <param name="other">The pair to compare with this pair.</param>
		/// <returns>True if equal, false if not equal. They are considered to be equal if the first elements are
		/// equal and the second elements are equal.</returns>
		/// <remarks>Warning, this function will box value types. If you need high-performance code,
		/// create a special type just for the two types you're putting together.</remarks>
		public bool Equals( Pair<FirstType, SecondType> other )
		{
			return object.Equals( this.First, other.First ) && object.Equals( this.Second, other.Second );
		}

		/// <summary>
		/// Compares two pairs for value equality.
		/// </summary>
		/// <param name="firstPair">The first pair to use in the comparison.</param>
		/// <param name="secondPair">The second pair to use in the comparison.</param>
		/// <returns>True if equal, false if not equal. They are considered to be equal if the first elements are
		/// equal and the second elements are equal.</returns>
		/// <remarks>Warning, this function will box value types. If you need high-performance code,
		/// create a special type just for the two types you're putting together.</remarks>
		public static bool operator ==( Pair<FirstType, SecondType> firstPair, Pair<FirstType, SecondType> secondPair )
		{
			return firstPair.Equals( secondPair );
		}

		/// <summary>
		/// Compares two pairs for value inequality.
		/// </summary>
		/// <param name="firstPair">The first pair to use in the comparison.</param>
		/// <param name="secondPair">The second pair to use in the comparison.</param>
		/// <returns>True if not equal, false if equal. They are considered to be equal if the first elements are
		/// equal and the second elements are equal.</returns>
		/// <remarks>Warning, this function will box value types. If you need high-performance code,
		/// create a special type just for the two types you're putting together.</remarks>
		public static bool operator !=( Pair<FirstType, SecondType> firstPair, Pair<FirstType, SecondType> secondPair )
		{
			return !firstPair.Equals( secondPair );
		}

		/// <summary>
		/// Gets a hash code for this pair.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks>Warning, this function will box value types. If you need high-performance code, create a special
		/// type just for the two types you're putting together.</remarks>
		public override int GetHashCode()
		{
			int hashOfFirst = ReferenceEquals( First, null ) ? 0 : First.GetHashCode();
			int hashOfSecond = ReferenceEquals( Second, null ) ? 0 : Second.GetHashCode();

			unchecked
			{
				return 37 + 23 * ( hashOfFirst + ( 23 * hashOfSecond ) ); // So (x, y) and (y, x) have different hashes
			}
		}

		/// <summary>
		/// Converts the value of this pair to a string.
		/// </summary>
		/// <returns>A string displaying the value of the pair in the form (x, y).</returns>
		public override string ToString()
		{
			return string.Format( "({0}, {1})", First, Second );
		}
	}
}
