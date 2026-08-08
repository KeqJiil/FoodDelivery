using FluentAssertions;
using FoodDelivery.IntegrationTest.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Aggregates;
using Payments.Domain.Enums;
using Payments.Domain.Ids;
using Payments.Infrastructure.Persistence;
using SharedKernel.Domain.Enums;
using SharedKernel.Domain.ValueObjects;

namespace FoodDelivery.IntegrationTest.Persistence.Payments;

public class PaymentPersistenceTests(MsSqlContainerFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task SaveAndReload_ShouldPersistPayment()
    {
        var orderRefId = new OrderRefId(Guid.NewGuid());
        var payment = Payment.Create(new PaymentId(Guid.NewGuid()), orderRefId, Money.Create(Currency.Usd, 25m).Ok!);

        await using (var writeContext = CreatePaymentsContext())
        {
            writeContext.Payments.Add(payment);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = CreatePaymentsContext();
        var reloaded = await readContext.Payments.FirstOrDefaultAsync(p => p.Id == payment.Id);

        reloaded.Should().NotBeNull();
        reloaded!.Status.Should().Be(PaymentStatus.Pending);
        reloaded.OrderRefId.Should().Be(orderRefId);
        reloaded.Amount.Should().Be(Money.Create(Currency.Usd, 25m).Ok!);
        reloaded.FailureReason.Should().BeNull();
    }

    [Fact]
    public async Task SecondSave_ShouldThrow_WhenBothContextsLoadedTheSameStalePayment()
    {
        var payment = Payment.Create(new PaymentId(Guid.NewGuid()), new OrderRefId(Guid.NewGuid()),
            Money.Create(Currency.Usd, 25m).Ok!);

        await using (var writeContext = CreatePaymentsContext())
        {
            writeContext.Payments.Add(payment);
            await writeContext.SaveChangesAsync();
        }

        await using var firstContext = CreatePaymentsContext();
        await using var secondContext = CreatePaymentsContext();

        var firstCopy = await firstContext.Payments.FirstAsync(p => p.Id == payment.Id);
        var secondCopy = await secondContext.Payments.FirstAsync(p => p.Id == payment.Id);

        firstCopy.Succeed();
        await firstContext.SaveChangesAsync();

        secondCopy.Fail("Card declined");

        DbUpdateConcurrencyException? caughtException = null;
        try
        {
            await secondContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            caughtException = ex;
        }

        caughtException.Should().NotBeNull();
    }

    private PaymentsDbContext CreatePaymentsContext() =>
        CreateContext<PaymentsDbContext>("payments", o => new PaymentsDbContext(o));
}
