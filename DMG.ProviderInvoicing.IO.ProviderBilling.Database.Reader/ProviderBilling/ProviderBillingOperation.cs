using System.Data;
using System.Diagnostics;
using System.Text;
using DMG.ProviderInvoicing.BL.Utility;
using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.DT.Domain.Map;
using DMG.ProviderInvoicing.DT.Domain.Rule;
using DMG.ProviderInvoicing.IO.Host;
using DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.Map;
using DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels;
using DMG.ProviderInvoicing.IO.Utility.Postgres;
using LanguageExt;
using LanguageExt.TypeClasses;
using Npgsql;
using NUnit.Framework.Interfaces;
using static DMG.ProviderInvoicing.DT.Domain.ErrorMessage;
using static DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.Map.ProviderBillingMapper;
using static LanguageExt.Prelude;
using Job = DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels.Job;
using MultiVisitJob = DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels.MultiVisitJob;
using ProcessingFee =
	DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling.TableModels.ProcessingFee;

namespace DMG.ProviderInvoicing.IO.ProviderBilling.Database.Reader.ProviderBilling;

/// <summary>
/// CRUD operations for job billing entity/table.
/// </summary>
internal class ProviderBillingOperation
{
	private readonly string _providerInvoicingConnectionString;

	public ProviderBillingOperation(string providerInvoicingConnectionString)
	{
		_providerInvoicingConnectionString = providerInvoicingConnectionString;
	}

	/// <summary>
	/// Function to retrieve a <see cref="JobBillingFalDatabasePersisted"/> from the database.
	/// </summary>
	/// <param name="providerBillingId">The job billing ID to look up.</param>
	/// <returns>Either an error message if the operation failed, or the <see cref="JobBillingFalDatabasePersisted"/> if successful.</returns>
	internal async Task<Either<ErrorMessage, DT.Domain.ProviderBilling>> RetrieveByIdAsync(
		ProviderBillingId providerBillingId)
	{
		try
		{
			OptionalTables optionalTables = await BuildOptionalTablesAsync(providerBillingId);
			return Right(BuildProviderBilling(optionalTables.Jobs, optionalTables.ProviderBillingOption, optionalTables,
				providerBillingId));
		}
		catch (Exception ex)
		{
			// we still need to have different catches for things like db unavailable etc
			return Either<ErrorMessage, DT.Domain.ProviderBilling>.Left(ErrorMessage.NewUnexpectedException(ex));
		}
	}

	internal async Task<bool> ContainsAsync(ProviderBillingId providerBillingId)
	{
		try
		{
			string sql = "select * from provider_billing.sp_check_provider_billing_exists (@id)";

			using var connection = new NpgsqlConnection(_providerInvoicingConnectionString);
			connection.Open();
			var cmd = new NpgsqlCommand(sql.ToString(), connection);
			NpgsqlCommandUtility.ParametersAddValueUuid(cmd.Parameters, "@id", providerBillingId.Value);
			bool exists =
				(bool)(await cmd.ExecuteScalarAsync() ??
					   false); //stored procedure always returns true/false but make the compiler happy

			return exists;
		}
		catch (Exception ex)
		{
			IoAdapterLogger.Exception(ex, ex.ToString());
			return false;
		}
	}

	internal async Task<bool> ExistForJobAsync(JobWorkId jobWorkId)
	{
		try
		{
			string sql = "select * from provider_billing.sp_check_provider_billing_exists_for_job (@id)";

			using var connection = new NpgsqlConnection(_providerInvoicingConnectionString);
			connection.Open();
			var cmd = new NpgsqlCommand(sql.ToString(), connection);
			NpgsqlCommandUtility.ParametersAddValueUuid(cmd.Parameters, "@id", jobWorkId.Value);
			bool exists =
				(bool)(await cmd.ExecuteScalarAsync() ??
					   false); //stored procedure always returns true/false but make the compiler happy

			return exists;
		}
		catch (Exception ex)
		{
			IoAdapterLogger.Exception(ex, ex.ToString());
			return false;
		}
	}


	internal async Task<Either<ErrorMessage, Lst<DT.Domain.JobPhoto>>> GetPhotosByProviderBillingIdAsync(
		ProviderBillingId providerBillingId)
	{
		try
		{
			// TODO: Instead of calling BuildOptionalTablesAsync, write a method which only gets the required tables.
			OptionalTables optionalTables = await BuildOptionalTablesAsync(providerBillingId);

			var photos = optionalTables.Photos;
			List<JobPhoto> photoOut = new List<DT.Domain.JobPhoto>();

			foreach (var photo in photos)
			{
				var meta = ToRecordMeta(photo.EntityId, EntityTypeToEntity(photo.EntityType), optionalTables);

				if (EntityTypeToEntity(photo.EntityType) == EntityType.SERVICE_LINE_ITEM)
				{
					Guid visitId = optionalTables.ServiceLineItems
						.Where(s => s.ItemId == photo.EntityId)
						.Select(v => v.VisitId)
						.FirstOrDefault();

					Guid? jobWorkId = optionalTables.Visits
						.Where(o => o.VisitId == visitId)
						.Select(o => o.JobWorkId)
						.FirstOrDefault();

					photoOut.Add(ToEntity(photo, photo.EntityId, visitId, jobWorkId ?? Guid.Empty, meta));
				}
				else if (EntityTypeToEntity(photo.EntityType) == EntityType.VISIT)
				{
					Guid? jobWorkId = optionalTables.Visits
						.Where(o => o.VisitId == photo.EntityId)
						.Select(o => o.JobWorkId)
						.FirstOrDefault();

					photoOut.Add(ToEntity(photo, Option<Guid>.None, photo.EntityId, jobWorkId ?? Guid.Empty, meta));
				}
				else if (EntityTypeToEntity(photo.EntityType) == EntityType.PROVIDER_BILLING)
				{
					photoOut.Add(ToEntity(photo, Option<Guid>.None, Guid.Empty, Guid.Empty, meta));
				}
				// below else for avoiding any error, should never execute
				else
				{
					photoOut.Add(ToEntity(photo, Option<Guid>.None, photo.EntityId, Guid.Empty, meta));
				}
			}

			return Right(photoOut.Freeze());
		}
		catch (Exception ex)
		{
			// we still need to have different catches for things like db unavailable etc
			return Either<ErrorMessage, Lst<DT.Domain.JobPhoto>>.Left(ErrorMessage.NewUnexpectedException(ex));
		}
	}


	internal async Task<Either<ErrorMessage, Lst<ProviderBillingId>>> GetPendingInvoicesForProviderId(
		ProviderOrgId providerOrgId)
	{
		try
		{
			string sql = "select * from provider_billing.sp_pending_provider_invoices (@id)";

			using var connection = new NpgsqlConnection(_providerInvoicingConnectionString);
			connection.Open();
			var cmd = new NpgsqlCommand(sql.ToString(), connection);
			NpgsqlCommandUtility.ParametersAddValueUuid(cmd.Parameters, "@id", providerOrgId.Value);
			var reader = await cmd.ExecuteReaderAsync();

			List<ProviderBillingId> ids = new List<ProviderBillingId>();
			while (await reader.ReadAsync())
			{
				ids.Add(new ProviderBillingId(reader.GetGuid("provider_billing_id")));
			}

			return ids.Freeze();
		}
		catch (Exception ex)
		{
			IoAdapterLogger.Exception(ex, ex.ToString());
			return Left<ErrorMessage>(NewUnexpectedException(ex));
		}
	}

	internal static DT.Domain.JobPhoto ToEntity(TableModels.Photo entity, Option<Guid> serviceLineId, Guid visitId,
		Guid jobWorkId, Option<RecordMeta> meta)
	{
		var serviceId = serviceLineId.Match(
			Some: e => new ServiceLineId(e),
			None: () => Option<ServiceLineId>.None
		);

		string photoChronology = entity.PhotoChronology ?? @"DATA_NOT_FOUND";
		string fileName = entity.FileName ?? @"DATA_NOT_FOUND";
		string? description = entity.Description;

		return new DT.Domain.JobPhoto(
			new JobPhotoBase(
				JobWorkId: new JobWorkId(jobWorkId),
				JobPhotoId: new JobPhotoId(entity.PhotoId),
				MimeType: NonEmptyText.NewUnsafe(entity.MimeType),
				PhotoChronology: ToProviderBillingPhotoChronology(photoChronology),
				FileName: NonEmptyText.NewUnsafe(fileName),
				Description: NonEmptyText.NewOptionUnvalidated(description),
				ServiceLineId: serviceId,
				VisitId: new VisitId(visitId)
			),
			meta.Match(
				Some: e => new RecordMeta(e.CreatedByUserId, e.CreatedOnDateTime, e.ModifiedByUserId,
					e.ModifiedOnDateTime),
				None: () => new RecordMeta(Option<UserId>.None, Option<DateTimeOffset>.None, Option<UserId>.None,
					Option<DateTimeOffset>.None))
		);
	}

	private async Task<OptionalTables> BuildOptionalTablesAsync(ProviderBillingId providerBillingId)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(Job.Sql);
		stringBuilder.AppendLine(TableModels.NonRoutineProviderBilling.Sql);
		stringBuilder.AppendLine(TableModels.RoutineProviderBilling.Sql);
		stringBuilder.AppendLine(ProviderBillingAssignment.Sql);
		stringBuilder.AppendLine(CatalogedMaterialPartLineItem.Sql);
		stringBuilder.AppendLine(MaterialPartLineItemRule.Sql);
		stringBuilder.AppendLine(NonCatalogedMaterialPartLineItem.Sql);
		stringBuilder.AppendLine(CatalogedEquipmentLineItem.Sql);
		stringBuilder.AppendLine(EquipmentLineItemRule.Sql);
		stringBuilder.AppendLine(NonCatalogedEquipmentLineItem.Sql);
		stringBuilder.AppendLine(LaborLineItem.Sql);
		stringBuilder.AppendLine(LaborJobAssignment.Sql);
		stringBuilder.AppendLine(LaborTimeAdjustment.Sql);
		stringBuilder.AppendLine(TechnicianLabor.Sql);
		stringBuilder.AppendLine(TripChargeLineItem.Sql);
		stringBuilder.AppendLine(TableModels.ProviderBillingTripChargeLineItem.Sql);
		stringBuilder.AppendLine(ProcessingFee.Sql);
		stringBuilder.AppendLine(Payment.Sql);
		stringBuilder.AppendLine(PaymentCreditCard.Sql);
		stringBuilder.AppendLine(PaymentElectronicFundTransfer.Sql);
		stringBuilder.AppendLine(TableModels.Photo.Sql);
		stringBuilder.AppendLine(TableModels.JobDocument.Sql);
		stringBuilder.AppendLine(Visit.Sql);
		stringBuilder.AppendLine(ServiceLineItem.Sql);
		stringBuilder.AppendLine(BillingLevelLineItem.Sql);
		stringBuilder.AppendLine(MetaData.Sql);
		stringBuilder.AppendLine(TableModels.JobBillingRuleMessage.Sql);
		stringBuilder.AppendLine(JobBillingRuleMessageVisibility.Sql);
		stringBuilder.AppendLine(JobBillingSubmissionSource.Sql);
		stringBuilder.AppendLine(Discount.Sql);
		stringBuilder.AppendLine(Dispute.Sql);
		stringBuilder.AppendLine(JobBillingDisputeConversation.Sql);
		stringBuilder.AppendLine(JobBillingDisputeRequest.Sql);
		stringBuilder.AppendLine(JobBillingDisputeResponse.Sql);
		stringBuilder.AppendLine(TableModels.Event.Sql);
		stringBuilder.AppendLine(TimeAndMaterialLineItem.Sql);
		stringBuilder.AppendLine(TableModels.WeatherWorks.Sql);
		stringBuilder.AppendLine(Adjustment.Sql);
		stringBuilder.AppendLine(PreProviderBilling.Sql);
		stringBuilder.AppendLine(MultiVisitJob.Sql);

		using var connection = new NpgsqlConnection(_providerInvoicingConnectionString);
		connection.Open();
		var cmd = new NpgsqlCommand(stringBuilder.ToString(), connection);
		NpgsqlCommandUtility.ParametersAddValueUuid(cmd.Parameters, "@id", providerBillingId.Value);
		NpgsqlDataReader reader = cmd.ExecuteReader();

		Lst<Job> jobs = await Job.ReadAsync(reader);
		reader.NextResult();
		Option<TableModels.NonRoutineProviderBilling> nonRoutieProviderBillingOption =
			await TableModels.NonRoutineProviderBilling.ReadAsync(reader);
		reader.NextResult();
		Option<TableModels.RoutineProviderBilling> routineProviderBillingOption =
			await TableModels.RoutineProviderBilling.ReadAsync(reader);
		reader.NextResult();
		Option<ProviderBillingAssignment> providerInvoiceAssignmentOption =
			await ProviderBillingAssignment.ReadAsync(reader);
		reader.NextResult();
		Lst<CatalogedMaterialPartLineItem> catalogMaterialPartLineItems =
			await CatalogedMaterialPartLineItem.ReadAsync(reader);
		reader.NextResult();
		Lst<MaterialPartLineItemRule> materialPartLineItemRules = await MaterialPartLineItemRule.ReadAsync(reader);
		reader.NextResult();
		Lst<NonCatalogedMaterialPartLineItem> nonCatalogMaterialPartLineItems =
			await NonCatalogedMaterialPartLineItem.ReadAsync(reader);
		reader.NextResult();
		Lst<CatalogedEquipmentLineItem> catalogEquipmentLineItems = await CatalogedEquipmentLineItem.ReadAsync(reader);
		reader.NextResult();
		Lst<EquipmentLineItemRule> equipmentLineItemRules = await EquipmentLineItemRule.ReadAsync(reader);
		reader.NextResult();
		Lst<NonCatalogedEquipmentLineItem> nonCatalogEquipmentLineItems =
			await NonCatalogedEquipmentLineItem.ReadAsync(reader);
		reader.NextResult();
		Lst<LaborLineItem> laborLineItems = await LaborLineItem.ReadAsync(reader);
		reader.NextResult();
		Lst<LaborJobAssignment> laborJobAssignments = await LaborJobAssignment.ReadAsync(reader);
		reader.NextResult();
		Lst<LaborTimeAdjustment> laborTimeAdjustments = await LaborTimeAdjustment.ReadAsync(reader);
		reader.NextResult();
		Lst<TechnicianLabor> technicianLabor = await TechnicianLabor.ReadAsync(reader);
		reader.NextResult();
		Lst<TripChargeLineItem> tripChargeLineItems = await TripChargeLineItem.ReadAsync(reader);
		reader.NextResult();
		Lst<TableModels.ProviderBillingTripChargeLineItem> providerBillingtripChargeLineItems = await TableModels.ProviderBillingTripChargeLineItem.ReadAsync(reader);
		reader.NextResult();
		Lst<ProcessingFee> processingFee = await ProcessingFee.ReadAsync(reader);
		reader.NextResult();
		Lst<Payment> payments = await Payment.ReadAsync(reader);
		reader.NextResult();
		Lst<PaymentCreditCard> paymentCreditCards = await PaymentCreditCard.ReadAsync(reader);
		reader.NextResult();
		Lst<PaymentElectronicFundTransfer> paymentElectronicFundTransfers =
			await PaymentElectronicFundTransfer.ReadAsync(reader);
		reader.NextResult();
		Lst<TableModels.Photo> photos = await TableModels.Photo.ReadAsync(reader);
		reader.NextResult();
		Lst<TableModels.JobDocument> jobDocuments = await TableModels.JobDocument.ReadAsync(reader);
		reader.NextResult();
		Lst<Visit> visits = await Visit.ReadAsync(reader);
		reader.NextResult();
		Lst<ServiceLineItem> serviceLineItems = await ServiceLineItem.ReadAsync(reader);
		reader.NextResult();
		Lst<BillingLevelLineItem> billingLevelLineItems = await BillingLevelLineItem.ReadAsync(reader);
		reader.NextResult();
		Lst<MetaData> metaDatas = await MetaData.ReadAsync(reader);
		reader.NextResult();
		Lst<TableModels.JobBillingRuleMessage> jobbillingRuleMessages =
			await TableModels.JobBillingRuleMessage.ReadAsync(reader);
		reader.NextResult();
		Lst<JobBillingRuleMessageVisibility> jobbillingRuleMessageVisibilities =
			await JobBillingRuleMessageVisibility.ReadAsync(reader);
		reader.NextResult();
		Lst<JobBillingSubmissionSource> jobbillingSubmissionSources =
			await JobBillingSubmissionSource.ReadAsync(reader);
		reader.NextResult();
		Lst<Discount> discountLineItems = await Discount.ReadAsync(reader);
		reader.NextResult();
		Lst<Dispute> jobBillingDispute = await Dispute.ReadAsync(reader);
		reader.NextResult();
		Lst<JobBillingDisputeConversation> jobBillingDisputeConversations =
			await JobBillingDisputeConversation.ReadAsync(reader);
		reader.NextResult();
		Lst<JobBillingDisputeRequest> jobBillingDisputeRequests = await JobBillingDisputeRequest.ReadAsync(reader);
		reader.NextResult();
		Lst<JobBillingDisputeResponse> jobBillingDisputeResponses = await JobBillingDisputeResponse.ReadAsync(reader);
		reader.NextResult();
		Lst<TableModels.Event> events = await TableModels.Event.ReadAsync(reader);
		reader.NextResult();
		Lst<TimeAndMaterialLineItem> timeAndMaterialLineItems =
			await TimeAndMaterialLineItem.ReadAsync(reader);
		reader.NextResult();
		Lst<TableModels.WeatherWorks> weatherworks = await TableModels.WeatherWorks.ReadAsync(reader);
		reader.NextResult();
		Lst<Adjustment> adjustments = await Adjustment.ReadAsync(reader);
		reader.NextResult();
		Option<PreProviderBilling> preProviderBilling = await PreProviderBilling.ReadAsync(reader);
		reader.NextResult();
		Lst<MultiVisitJob> multiVisitJob = await MultiVisitJob.ReadAsync(reader);
		reader.NextResult();
		Option<TableModels.ProviderBilling> providerBillingOption =
			MergeToProviderBilling(nonRoutieProviderBillingOption, routineProviderBillingOption, preProviderBilling);

		OptionalTables optionalTables = new OptionalTables(
			jobs,
			nonRoutieProviderBillingOption,
			routineProviderBillingOption,
			providerBillingOption,
			providerInvoiceAssignmentOption,
			catalogMaterialPartLineItems,
			materialPartLineItemRules,
			nonCatalogMaterialPartLineItems,
			catalogEquipmentLineItems,
			equipmentLineItemRules,
			nonCatalogEquipmentLineItems,
			laborLineItems,
			laborJobAssignments,
			laborTimeAdjustments,
			technicianLabor,
			tripChargeLineItems,
			providerBillingtripChargeLineItems,
			processingFee,
			payments,
			paymentCreditCards,
			paymentElectronicFundTransfers,
			photos,
			jobDocuments,
			visits,
			serviceLineItems,
			billingLevelLineItems,
			metaDatas,
			jobbillingRuleMessages,
			jobbillingRuleMessageVisibilities,
			jobbillingSubmissionSources,
			discountLineItems,
			jobBillingDispute,
			jobBillingDisputeConversations,
			jobBillingDisputeRequests,
			jobBillingDisputeResponses,
			events,
			timeAndMaterialLineItems,
			weatherworks,
			adjustments,
			preProviderBilling,
			multiVisitJob
		);

		return optionalTables;
	}

	// TODO: After separating routine and non-routine data, modify this function accordingly.
	// non_routine_provider_billing table is currently not populated.
	// All records are stored in routine_provider_billing and distinguished by BillingType
	private static Option<TableModels.ProviderBilling> MergeToProviderBilling(
		Option<NonRoutineProviderBilling> nonRoutineProviderBillingOption,
		Option<RoutineProviderBilling> routineProviderBillingOption,
		Option<PreProviderBilling> preProviderBillingOption)
	{
		string billingTypeString = preProviderBillingOption.Match(
			e => e.BillingType,
			() => "");

		BillingType billingType;
		if (billingTypeString == "ROUTINE_BILLING_TYPE")
		{
			billingType = BillingType.NewRoutine(RoutineBillingType.PerOccurrence);
		}
		else
		{
			billingType = BillingType.NewNonRoutine(NonRoutineBillingType.Standard);
		}

		return routineProviderBillingOption.Bind(f => ToProviderBilling(f, billingType));
	}

	private static Either<ErrorMessage, DT.Domain.ProviderBilling> BuildProviderBilling(Lst<Job> jobs,
		Option<TableModels.ProviderBilling> providerBillingOption, OptionalTables optionalTables,
		ProviderBillingId providerBillingId) =>
		BuildProviderBilling(Optional(jobs.FirstOrDefault()), providerBillingOption, optionalTables, providerBillingId);

	private static Either<ErrorMessage, DT.Domain.ProviderBilling> BuildProviderBilling(Option<Job> jobOption,
		Option<TableModels.ProviderBilling> providerBillingOption, OptionalTables optionalTables,
		ProviderBillingId providerBillingId) =>
		providerBillingOption.Match(
			providerBilling => BuildProviderBilling(jobOption, providerBilling, optionalTables, providerBillingId),
			() => Left<ErrorMessage, DT.Domain.ProviderBilling>(ErrorMessage.ProviderBillingNotFound)
		);

	private static DT.Domain.ProviderBilling BuildProviderBilling(Option<Job> jobOption,
		TableModels.ProviderBilling providerBilling, OptionalTables optionalTables,
		ProviderBillingId providerBillingId)
	{
		string billingType = optionalTables.PreProviderBillings.Match(billing => billing.BillingType, string.Empty);
		string billingSubType = optionalTables.PreProviderBillings.Match(billingSubType => billingSubType.BillingSubType, string.Empty);

		return new DT.Domain.ProviderBilling(
			providerBillingId,
			new JobBillingVersion(Convert.ToUInt32(providerBilling.Version)),
			optionalTables.PreProviderBillings.Match(
				preProviderBilling => preProviderBilling.IsCIWO,
				false),
			GetBillingContractType(billingType),
			new TicketId(providerBilling.TicketId),
			NonEmptyText.NewUnsafe(providerBilling.TicketNumber),
			new ProviderOrgId(providerBilling.ProviderOrgId),
			new CustomerId(providerBilling.CustomerId),
			new PropertyId(providerBilling.PropertyId),
			new ServiceLineId(providerBilling.ServiceLineId),
			ProviderBilling.Map.ProviderBillingMapper.CostingSchemeToEntity(providerBilling.CostingScheme),
			ProviderBilling.Map.ProviderBillingMapper.ProviderBillingStatusToEntity(providerBilling.Status),
			providerBilling.TotalCost,
			optionalTables.ProviderInvoiceAssignmentOption.Match(
				e => ProviderBilling.Map.ProviderBillingMapper.AssignedToEntity(e.AssignedTo),
				ProviderBillingAssignee.Provider),
			SourceSystem: SourceSystem.ProviderBilling,
			GetBillingType(billingType, billingSubType),
			// optional scalars
			providerBilling.BillingType.IsNonRoutine
				? new JobBillingId(providerBilling.ProviderBillingId)
				: Option<JobBillingId>.None,
			NonEmptyText.NewOptionUnvalidated(providerBilling
				.ProviderBillingNumber), //this is only populated when it is routine so it will be empty in nonroutine like we want
			NonEmptyText.NewOptionUnvalidated(providerBilling.JobSummary),
			NonEmptyText.NewOptionUnvalidated(providerBilling.Description), //TODO is this correct for detail?
			NonEmptyText.NewOptionUnvalidated(providerBilling.Notes),
			NonEmptyText.NewOptionUnvalidated(providerBilling.ProviderInvoiceNumber),
			optionalTables.ProviderInvoiceAssignmentOption.Match(
				providerInvoiceAssignment => Optional<DateTime>(providerInvoiceAssignment.ProviderFirstSubmittedOnDate)
					.Match(
						provFirstSubmitDateTime => new DateTimeOffset(provFirstSubmitDateTime),
						Option<DateTimeOffset>.None),
				Option<DateTimeOffset>.None),
			optionalTables.ProviderInvoiceAssignmentOption.Match(
				providerInvoiceAssignment => Optional<DateTime>(providerInvoiceAssignment.ProviderLastSubmittedOnDate)
					.Match(
						provLastSubmitDateTime => new DateTimeOffset(provLastSubmitDateTime),
						Option<DateTimeOffset>.None),
				Option<DateTimeOffset>.None),
			Optional<DateTimeOffset>(providerBilling.InvoiceCreatedAt),
			Optional<Guid>(providerBilling.PsaId)
				.Match(
					guid => new PsaId(guid),
					Option<PsaId>.None),
			// required sections
			ProviderBilling.Map.ProviderBillingMapper.ToRecordMeta(providerBillingId.Value, EntityType.PROVIDER_BILLING,
				optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToMaterialPart(providerBillingId, optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToEquipment(providerBillingId, optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToLabor(providerBillingId, optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToTechnicianTripCharge(optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToJobFlatRate(providerBillingId, optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToMaterialPartFlatRate(providerBillingId, optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToEquipmentFlatRate(providerBillingId, optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToVisitDetail(optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToBillingDetail(optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToProcessingFee(optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToPayment(optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToProviderBillingJobGroup(optionalTables), //TODO check this
			ProviderBilling.Map.ProviderBillingMapper.ToProviderBillingDiscount(optionalTables),

			// optional sections
			ProviderBilling.Map.ProviderBillingMapper.ToSubmissionDetailLatest(optionalTables),
			Additional: Option<JobBillingAdditional>.None, //TODO I don't think this is in the database
			ProviderBilling.Map.ProviderBillingMapper.ToEvent(optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToProviderBillingTripChargeLineItem(optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToWeatherWorks(optionalTables),
			// required collection
			ProviderBilling.Map.ProviderBillingMapper.ToJobBillingRuleMessage(providerBillingId.Value,
				EntityType.PROVIDER_BILLING, optionalTables),
			ProviderBilling.Map.ProviderBillingMapper.ToMultiVisitJob(optionalTables.MultiVisitJobs)
		);
	}

	private static BillingType GetBillingType(string billingType, string billingSubType) =>
		billingType switch
		{
			"NON_ROUTINE_BILLING_TYPE" => GetBillingNonRoutineSubType(billingSubType),
			"ROUTINE_BILLING_TYPE" => GetBillingRoutineSubType(billingSubType),
			_ => BillingType.Undefined
		};
	private static BillingContractType GetBillingContractType(string billingType) =>
		billingType switch
		{
			"NON_ROUTINE_BILLING_TYPE" => BillingContractType.NonRoutine,
			"ROUTINE_BILLING_TYPE" => BillingContractType.Routine,
			_ => BillingContractType.Undefined
		};

	private static BillingType GetBillingRoutineSubType(string billingSubType) =>
		billingSubType switch
		{
			"SEASONAL_FLATRATE" => BillingType.NewRoutine(RoutineBillingType.SeasonalFlatRate),
			"SEASONAL_VARIABLE" => BillingType.NewRoutine(RoutineBillingType.SeasonalVariable),
			"SEASONAL_TIERED" => BillingType.NewRoutine(RoutineBillingType.SeasonalTiered),
			"SEASONAL_HYBRID" => BillingType.NewRoutine(RoutineBillingType.SeasonalHybrid),
			"TIMEANDMATERIAL" => BillingType.NewRoutine(RoutineBillingType.TimeAndMaterial),
			"PEROCCURRENCE" => BillingType.NewRoutine(RoutineBillingType.PerOccurrence),
			"PEREVENT" => BillingType.NewRoutine(RoutineBillingType.PerEvent),
			_ => BillingType.NewRoutine(RoutineBillingType.Undefined)
		};
	private static BillingType GetBillingNonRoutineSubType(string billingSubType) =>
		billingSubType switch
		{
			"STANDARD" => BillingType.NewNonRoutine(NonRoutineBillingType.Standard),
			"SNOW" => BillingType.NewNonRoutine(NonRoutineBillingType.Snow),
			"LOTANDLAND" => BillingType.NewNonRoutine(NonRoutineBillingType.LotAndLand),
			_ => BillingType.NewRoutine(RoutineBillingType.Undefined)
		};
}