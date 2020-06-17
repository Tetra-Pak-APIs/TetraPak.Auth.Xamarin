using System;

namespace TetraPak.Auth.Xamarin.common
{
    // todo Consider moving BoolValue to a common NuGet package to be referenced instead
    /// <summary>
    ///   A boolean value that can also carry another value. This is
    ///   very useful as a typical return value where you need an indication
    ///   for "success" and, when successful, a value.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of value carried by the <see cref="BoolValue{T}"/>.
    /// </typeparam>
    /// <remarks>
    ///   Instances of type <see cref="BoolValue{T}"/> can be implicitly cast to
    ///   a <c>bool</c> value. Very useful for testing purposes.
    /// </remarks>
    public class BoolValue<T> : BoolValue
    {
        /// <summary>
        ///   The value carried by the object.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        ///   Creates a <see cref="BoolValue{T}"/> that equals <c>true</c> when cast to a
        ///   <see cref="bool"/> value.
        /// </summary>
        /// <param name="value">
        ///   The value (of type <typeparamref name="T"/>) to be carried by the
        ///   new <see cref="BoolValue{T}"/> object.
        /// </param>
        /// <returns>
        ///   A <see cref="BoolValue{T}"/> that represents a <c>true</c> value when
        ///   cast as a <see cref="Boolean"/> while also carrying a specified value.
        /// </returns>
        public static BoolValue<T> Success(T value) => new BoolValue<T>(true, null, null) { Value = value };

        /// <summary>
        ///   Creates a <see cref="BoolValue{T}"/> that equals <c>false</c> when cast to a
        ///   <see cref="bool"/> value.
        /// </summary>
        /// <param name="message">
        ///   (optional)<br/>
        ///   A message to be carried by the <see cref="BoolValue{T}"/> object
        ///   (useful for error handling).
        /// </param>
        /// <param name="exception">
        ///   (optional)<br/>
        ///   An <see cref="Exception"/> to be carried by the <see cref="BoolValue{T}"/> object
        ///   (useful for error handling).
        /// </param>
        /// <returns>
        ///   A <see cref="BoolValue{T}"/> that represents a <c>true</c> value when
        ///   cast as a <see cref="Boolean"/> while also carrying a specified value.
        /// </returns>
        public static BoolValue<T> Fail(string message = null, Exception exception = null) => new BoolValue<T>(false, message, exception);

        BoolValue(bool evaluated, string message, Exception exception) : base(evaluated, message, exception)
        {
        }
    }

    /// <summary>
    ///   An abstract base class for a boolean value. 
    /// </summary>
    public abstract class BoolValue
    {
        /// <summary>
        ///   Gets the value used when objects of this class is cast
        ///   to a <see cref="bool"/> value.
        /// </summary>
        protected bool Evaluated { get; }

        /// <summary>
        ///   A message to be carried by the <see cref="BoolValue{T}"/> object
        ///   (useful for error handling).
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///   An <see cref="Exception"/> to be carried by the <see cref="BoolValue{T}"/> object
        ///   (useful for error handling).
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        ///   Implicitly casts the <see cref="BoolValue"/> to a <see cref="bool"/> value.
        /// </summary>
        /// <param name="self"></param>
        public static implicit operator bool(BoolValue self) => self.Evaluated;

        /// <summary>
        ///   Initializes a <see cref="BoolValue"/>.
        /// </summary>
        /// <param name="evaluated">
        ///   Initializes the <see cref="Evaluated"/> property.
        /// </param>
        /// <param name="message">
        ///   Initializes the <see cref="Message"/> property.
        /// </param>
        /// <param name="exception">
        ///   Initializes the <see cref="Exception"/> property.
        /// </param>
        protected BoolValue(bool evaluated, string message, Exception exception)
        {
            Evaluated = evaluated;
            Message = message;
            Exception = exception;
        }
    }
}
