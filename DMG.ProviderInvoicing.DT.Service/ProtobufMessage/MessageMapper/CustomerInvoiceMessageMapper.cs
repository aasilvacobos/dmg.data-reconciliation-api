using DMG.ProviderInvoicing.DT.Domain;
using DMG.ProviderInvoicing.BL.Utility;
using LanguageExt;
using static LanguageExt.Prelude;
using static DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper.MessageMapperUtility;

namespace DMG.ProviderInvoicing.DT.Service.ProtobufMessage.MessageMapper;

public static class CustomerInvoiceMessageMapper
{
    public static DT.Domain.CustomerInvoice ToEntity(DMG.Finance.CustomerInvoice customerInvoice) =>
        new Domain.CustomerInvoice(
                new CustomerInvoiceExternalId(ParseGuidStringDefaultToEmptyGuid(customerInvoice.ExternalId)),
                new TicketId(ParseGuidStringDefaultToEmptyGuid(customerInvoice.DmgTicketId)),
                NonEmptyText.NewUnsafe(customerInvoice.DmgCustomerInvoiceStatus.DefaultIfNullOrWhiteSpace(DefaultRequiredStringValueIfMissing)),
                customerInvoice.ItemLineLineNumber,
                customerInvoice.MaxItemCount);

}