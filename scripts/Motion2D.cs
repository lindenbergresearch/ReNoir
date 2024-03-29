#region header

// 
//    _____
//   (, /   )            ,
//     /__ /  _ __   ___   __
//  ) /   \__(/_/ (_(_)_(_/ (_  CORE LIBRARY
// (_/ ______________________________________/
// 
// 
// Renoir Core Library for the Godot Game-Engine.
// Copyright 2020-2022 by Lindenberg Research.
// 
// www.lindenberg-research.com
// www.godotengine.org
// 

#endregion

#region

using Godot;

#endregion

namespace Renoir {

	/// <summary>
	///     Represents a motion as vector containing additional functions
	/// </summary>
	public class Motion2D {

		/// <summary>
		///     Normal vector for standard bottom ground
		/// </summary>
		public static readonly Vector2 FLOOR_NORMAL = new(0, -1);

		// internal velocity
		private Vector2 velocity;


		/// <summary>
		///     Create a new Motion2D by some Vector2
		/// </summary>
		/// <param name="velocity"></param>
		public Motion2D(Vector2 velocity) {
			Velocity = velocity;
		}


		/// <summary>
		///     Represents the velocity of the 2D motion as Vector2
		/// </summary>
		public Vector2 Velocity {
			get => velocity;
			set => velocity = value;
		}


		/// <summary>
		///     Returns the X part of the velocity vector
		/// </summary>
		public float X {
			get => velocity.x;
			set => velocity.x = value;
		}


		/// <summary>
		///     Returns the Y part of the velocity vector
		/// </summary>
		public float Y {
			get => velocity.y;
			set => velocity.y = value;
		}


		// Shorthand direction states
		public bool Stopped => velocity.Abs().Length() == 0;
		public bool MovingUp => velocity.y < 0;
		public bool MovingDown => velocity.y > 0;
		public bool MovingLeft => velocity.x < 0;
		public bool MovingRight => velocity.x > 0;
		public bool MovingHorizontal => MovingRight || MovingLeft;
		public bool MovingVertical => MovingUp || MovingDown;


		/// <summary>
		///     Returns the absolute values of both vector components
		/// </summary>
		/// <returns></returns>
		public Motion2D Abs() {
			var @this = this;
			velocity = velocity.Abs();
			return @this;
		}


		/// <summary>
		///     Reset velocity vector
		/// </summary>
		public void Reset() {
			velocity.x = 0;
			velocity.y = 0;
		}


		/// <summary>
		/// Apply a motion impulse.
		/// </summary>
		/// <param name="motion2D"></param>
		public void Impulse(Motion2D motion2D) {
			velocity += motion2D;
		}


		/// <summary>
		/// Apply a motion impulse.
		/// </summary>
		/// <param name="motion"></param>
		public void Impulse(Vector2 motion) {
			velocity += motion;
		}


		/// <summary>
		///     Return vector information as string
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return $"Velocity({velocity.x}, {velocity.y})";
		}


		/// <summary>
		///     Converts a Vector2 -> Motion2D
		/// </summary>
		/// <param name="vector2"></param>
		/// <returns></returns>
		public static implicit operator Motion2D(Vector2 vector2) {
			return new Motion2D(vector2);
		}


		/// <summary>
		///     Converts a Tuple2 -> Motion2D
		/// </summary>
		/// <param name="tuple"></param>
		/// <returns></returns>
		public static implicit operator Motion2D((int x, int y) tuple) {
			var (x, y) = tuple;
			return new Motion2D(new Vector2(x, y));
		}


		/// <summary>
		///     Converts a Motion2D -> Vector2
		/// </summary>
		/// <param name="motion2D"></param>
		/// <returns></returns>
		public static implicit operator Vector2(Motion2D motion2D) {
			return new Vector2(motion2D.Velocity);
		}

	}

}
