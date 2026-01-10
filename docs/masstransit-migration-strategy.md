# MassTransit Migration Strategy

## Executive Summary

MassTransit v9.x requires expensive commercial licensing. This project currently uses v8.5.6 (open-source) which is supported through 2026. We should plan migration to a free alternative before EOL.

## Current Situation

### What We're Using
- **MassTransit 8.5.6** (open-source, Apache 2.0)
- **MassTransit.Redis 8.5.6** for Redis transport
- Used for event-driven messaging between services

### Current Usage in Hickory
Based on codebase analysis:
- Email notifications (EmailNotificationConsumer)
- SignalR real-time notifications (SignalRNotificationConsumer)
- Webhook notifications (WebhookNotificationConsumer)
- Ticket events (TicketCreated, TicketUpdated, TicketAssigned, CommentAdded)

### MassTransit v8.x Support
- ✅ Security patches through 2026
- ✅ Bug fixes for critical issues
- ❌ No new features
- ❌ .NET 10 support backported (limited)
- ❌ No path to v9.x without commercial license

## The Problem

### MassTransit v9.x Licensing
- Requires commercial license from Massient
- Pricing not publicly disclosed (contact sales)
- Reports suggest **very expensive** for small teams
- Not viable for open-source or small projects

### Timeline Pressure
- v8.x EOL: December 2026
- Must migrate before then
- Need time for testing and validation

## Recommended Alternative: Rebus

### Why Rebus?

**✅ Advantages**:
1. **Free & Open Source** (MIT license)
2. **Similar API** to MassTransit (easier migration)
3. **Redis Support** (we're using Redis transport)
4. **Active Development** (regular updates)
5. **Good Documentation** (migration guides available)
6. **Lightweight** (less overhead than MassTransit)
7. **Proven** (used in production by many companies)

**📊 Comparison**:
| Feature | MassTransit v8.x | MassTransit v9.x | Rebus |
|---------|------------------|------------------|-------|
| License | Apache 2.0 (Free) | Commercial (Paid) | MIT (Free) |
| Redis Support | ✅ | ✅ | ✅ |
| Active Development | ❌ (EOL 2026) | ✅ | ✅ |
| .NET 10 Support | ⚠️ Backported | ✅ Native | ✅ Native |
| Community | Large | Large | Medium |
| Cost | Free | $$$$ | Free |
| Learning Curve | - | Easy (same) | Medium |

### Rebus Features
- Pub/Sub messaging
- Sagas (workflow orchestration)
- Retries and error handling
- Message routing
- Multiple transport options (Redis, RabbitMQ, Azure Service Bus, SQL, In-Memory)
- Dependency injection integration
- Async/await support

## Migration Plan

### Phase 1: Research & Planning (Q1 2026)
- [ ] Evaluate Rebus in depth
- [ ] Create proof-of-concept
- [ ] Document API differences
- [ ] Identify migration challenges
- [ ] Estimate effort (likely 2-3 weeks)

### Phase 2: Implementation (Q2-Q3 2026)
- [ ] Create new Rebus infrastructure layer
- [ ] Migrate one consumer as POC
- [ ] Run both MassTransit and Rebus in parallel
- [ ] Migrate remaining consumers
- [ ] Update tests
- [ ] Performance testing

### Phase 3: Validation (Q3 2026)
- [ ] Full regression testing
- [ ] Load testing
- [ ] Monitor in staging
- [ ] Fix any issues

### Phase 4: Production (Q4 2026)
- [ ] Deploy to production
- [ ] Monitor closely
- [ ] Keep MassTransit as fallback initially
- [ ] Remove MassTransit after validation

## Code Migration Example

### Current MassTransit Code

```csharp
// Configuration
services.AddMassTransit(x =>
{
    x.AddConsumer<TicketCreatedConsumer>();
    
    x.UsingRedis((context, cfg) =>
    {
        cfg.Host("localhost:6379");
        cfg.ConfigureEndpoints(context);
    });
});

// Consumer
public class TicketCreatedConsumer : IConsumer<TicketCreated>
{
    public async Task Consume(ConsumeContext<TicketCreated> context)
    {
        var message = context.Message;
        // Handle message
    }
}

// Publishing
await _publishEndpoint.Publish(new TicketCreated { /* ... */ });
```

### Equivalent Rebus Code

```csharp
// Configuration
services.AddRebus(configure => configure
    .Transport(t => t.UseRedis("localhost:6379", "hickory-messages"))
    .Routing(r => r.TypeBased()
        .Map<TicketCreated>("ticket-events")));

services.AddRebusHandler<TicketCreatedConsumer>();

// Consumer (Handler in Rebus)
public class TicketCreatedConsumer : IHandleMessages<TicketCreated>
{
    public async Task Handle(TicketCreated message)
    {
        // Handle message (similar logic)
    }
}

// Publishing
await _bus.Publish(new TicketCreated { /* ... */ });
```

### Key Differences
1. `IConsumer<T>` → `IHandleMessages<T>`
2. `Consume(ConsumeContext<T>)` → `Handle(T message)`
3. Configuration API slightly different
4. Routing must be explicit
5. No automatic endpoint configuration

## Alternative Options

### Option 2: Wolverine
**Pros**:
- Modern, actively developed
- Built-in transactional outbox
- Good for CQRS patterns
- Free and open-source

**Cons**:
- Less mature than Rebus
- Smaller community
- Different API (more migration work)

### Option 3: Direct Redis Implementation
**Pros**:
- Full control
- No framework overhead
- Simple for basic scenarios

**Cons**:
- More code to write
- Implement retries, error handling manually
- No built-in patterns (Saga, etc.)
- More maintenance burden

### Option 4: Stay on MassTransit v8.x
**Pros**:
- No migration needed
- Familiar codebase

**Cons**:
- EOL December 2026
- No future .NET support
- Security risk after 2026
- Technical debt accumulation

## Cost-Benefit Analysis

### Migration Cost
- Developer time: ~2-3 weeks
- Testing time: ~1 week
- Risk: Medium (well-documented migration path)
- **Total Cost**: ~$15-20k in developer time

### Stay on MassTransit v9.x Cost
- License cost: Unknown (requires sales contact)
- Reports suggest: $10k-50k+/year
- Ongoing annual cost
- **Total Cost**: $50k-250k over 5 years

### ROI
- Migration pays for itself in first year
- Avoid vendor lock-in
- Future-proof architecture

## Recommendation

### Short Term (2026)
✅ **Keep MassTransit v8.5.6** (current PR)
- Supported through 2026
- Works with .NET 10
- No immediate risk
- Buys time for proper migration

### Medium Term (Q2-Q4 2026)
✅ **Migrate to Rebus**
- Plan migration for Q2 2026
- Execute in Q3 2026
- Deploy Q4 2026
- Before MassTransit v8.x EOL

### Long Term (2027+)
✅ **Use Rebus**
- Free, open-source
- Active development
- No licensing concerns
- Future-proof

## Action Items

### Immediate (This PR)
- [x] Update to MassTransit 8.5.6
- [x] Document concern
- [x] Create migration strategy document

### Q1 2026
- [ ] Create GitHub issue for Rebus migration
- [ ] Research Rebus in detail
- [ ] Build POC with Rebus
- [ ] Present findings to team

### Q2 2026
- [ ] Approve migration plan
- [ ] Begin implementation
- [ ] Create side-by-side comparison tests

### Q3 2026
- [ ] Complete migration
- [ ] Full testing
- [ ] Staging deployment

### Q4 2026
- [ ] Production deployment
- [ ] Monitor and validate
- [ ] Remove MassTransit dependency

## Risk Mitigation

### Technical Risks
- **Migration bugs**: Run both systems in parallel initially
- **Performance differences**: Load test before production
- **Message loss**: Implement monitoring and alerting

### Business Risks
- **Downtime**: Blue-green deployment strategy
- **Feature delays**: Plan migration during slower period
- **Team knowledge**: Training on Rebus before migration

## Success Metrics

- ✅ Zero message loss during migration
- ✅ Equal or better performance vs MassTransit
- ✅ All consumers migrated successfully
- ✅ No production incidents
- ✅ Cost savings: $10k+/year

## Resources

- [Rebus Official Documentation](https://github.com/rebus-org/Rebus)
- [MassTransit to Rebus Migration Guide](https://github.com/rebus-org/Rebus/wiki/Migration-from-other-frameworks)
- [Rebus Redis Transport](https://github.com/rebus-org/Rebus.Redis)
- [Comparison: Rebus vs MassTransit](https://code-maze.com/aspnetcore-comparison-of-rebus-nservicebus-and-masstransit/)

## Conclusion

**MassTransit v8.5.6 is safe through 2026**, but we need a migration plan. **Rebus is the recommended alternative** due to its free license, similar API, and active development. Migration should happen in Q2-Q4 2026, well before the v8.x EOL.

This strategic decision saves significant licensing costs while maintaining functionality and avoiding vendor lock-in.
