using System;
using LanguageExt;
using static LanguageExt.Prelude;

namespace DMG.ProviderInvoicing.BL.Utility;

/// Wrapper for a string value that cannot be null, empty string or white space (i.e., a required string).
public abstract record NonEmptyText(string Value)
{
	/// Attempt to construct a NonEmptyText. Null, empty string or white space will return error.
	public static Either<string, NonEmptyText> New(string stringValue) =>
		string.IsNullOrWhiteSpace(stringValue)
			? Left(@"Value cannot be null, empty string or white space.")
			: Right(new NonEmptyTextImpl(stringValue) as NonEmptyText);

	/// Construct a NonEmptyText with no validation for null, empty string or white space.
	public static NonEmptyText NewUnsafe(string stringValue) =>
		new NonEmptyTextImpl(stringValue) as NonEmptyText;

	/// Create an option of NonEmptyText from a nullable on string value if it is not an empty string or white space
	public static Either<string, Option<NonEmptyText>> NewOption(string? stringValue) =>
		Optional(stringValue)
			.Match(x => x.Trim().Equals(string.Empty)
					? Either<string, Option<NonEmptyText>>.Left(@"Value cannot be empty string or white space.")
					: Either<string, Option<NonEmptyText>>.Right(Some(NonEmptyText.NewUnsafe(x))),
				() => Either<string, Option<NonEmptyText>>.Right(Option<NonEmptyText>.None));

	/// Unconditionally create an option of NonEmptyText from a nullable string value
	public static Option<NonEmptyText> NewOptionUnvalidated(string? stringValue) =>
		string.IsNullOrWhiteSpace(stringValue)
			? Option<NonEmptyText>.None
			: new NonEmptyTextImpl(stringValue);

	private record NonEmptyTextImpl(string Value) : NonEmptyText(Value);
}