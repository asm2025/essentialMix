using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace asm.Core.Data.Entity.Exceptions
{
	[Serializable]
	public class DbEntityValidationException : DataException
	{
		private ICollection<ValidationResult> _entityValidationResults;

		/// <summary>
		/// Initializes a new instance of DbEntityValidationException.
		/// </summary>
		public DbEntityValidationException()
		  : this("Validation failed")
		{
		}

		/// <summary>
		/// Initializes a new instance of DbEntityValidationException.
		/// </summary>
		/// <param name="message"> The exception message. </param>
		public DbEntityValidationException(string message)
		  : this(message, Enumerable.Empty<ValidationResult>())
		{
		}

		/// <summary>
		/// Initializes a new instance of DbEntityValidationException.
		/// </summary>
		/// <param name="message"> The exception message. </param>
		/// <param name="entityValidationResults"> Validation results. </param>
		public DbEntityValidationException(string message, [NotNull] IEnumerable<ValidationResult> entityValidationResults)
		  : base(message)
		{
			InitializeValidationResults(entityValidationResults);
		}

		/// <summary>
		/// Initializes a new instance of DbEntityValidationException.
		/// </summary>
		/// <param name="message"> The exception message. </param>
		/// <param name="innerException"> The inner exception. </param>
		public DbEntityValidationException(string message, Exception innerException)
		  : this(message, Enumerable.Empty<ValidationResult>(), innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of DbEntityValidationException.
		/// </summary>
		/// <param name="message"> The exception message. </param>
		/// <param name="entityValidationResults"> Validation results. </param>
		/// <param name="innerException"> The inner exception. </param>
		public DbEntityValidationException(string message, [NotNull] IEnumerable<ValidationResult> entityValidationResults, Exception innerException) 
		  : base(message, innerException)
		{
			InitializeValidationResults(entityValidationResults);
		}

		/// <summary>
		/// Initializes a new instance of the DbEntityValidationException class with the specified serialization information and context.
		/// </summary>
		/// <param name="info"> The data necessary to serialize or deserialize an object. </param>
		/// <param name="context"> Description of the source and destination of the specified serialized stream. </param>
		protected DbEntityValidationException([NotNull] SerializationInfo info, StreamingContext context)
		  : base(info, context)
		{
			_entityValidationResults = (ICollection<ValidationResult>)info.GetValue(nameof(EntityValidationErrors), typeof(ICollection<ValidationResult>));
		}

		/// <summary>Validation results.</summary>
		public IEnumerable<ValidationResult> EntityValidationErrors => _entityValidationResults;

		/// <summary>
		/// Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
		/// </summary>
		/// <param name="info"> The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
		/// <param name="context"> The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("EntityValidationErrors", _entityValidationResults, typeof(ICollection<ValidationResult>));
		}

		private void InitializeValidationResults([NotNull] IEnumerable<ValidationResult> entityValidationResults)
		{
			_entityValidationResults = entityValidationResults as ICollection<ValidationResult> ?? entityValidationResults.ToList();
		}
	}
}
