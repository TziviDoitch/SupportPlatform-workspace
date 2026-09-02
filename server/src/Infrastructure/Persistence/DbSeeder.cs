using Microsoft.EntityFrameworkCore;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Persistence;

/// <summary>
/// Deterministic, idempotent seed: reference lists + the filter-field registry (data, not code —
/// §8 Q1), two tenants, three users, ~40 submitting bodies and ~500 support requests in a
/// deliberately skewed distribution. Reproducible across runs via a fixed RNG seed.
/// </summary>
public static class DbSeeder
{
    private const int RngSeed = 20240901;

    // Demo credential documented in docs/contracts/api-contract.md §1 — not a secret.
    private const string DemoPassword = "pass";

    private static readonly (string Code, string Label)[] Domains =
        [("culture", "תרבות"), ("sport", "ספורט")];

    private static readonly (string Code, string Label)[] BodyTypes =
        [("association", "עמותה"), ("company", "חברה")];

    private static readonly (string Code, string Label)[] Statuses =
        [("approved", "מאושר"), ("pending", "בבדיקה"), ("rejected", "נדחה")];

    private static readonly (string Code, string Label)[] Districts =
        [("north", "צפון"), ("center", "מרכז"), ("south", "דרום")];

    private static readonly int[] Years = [2023, 2024, 2025];

    public static void Seed(SupportPlatformDbContext db)
    {
        if (db.SupportRequests.IgnoreQueryFilters().Any())
            return;

        SeedReferences(db);
        SeedRegistry(db);
        db.SaveChanges();

        var tenants = SeedTenants(db);
        SeedUsers(db);
        db.SaveChanges();

        var rng = new Random(RngSeed);
        var bodies = SeedBodies(db, rng);
        db.SaveChanges();

        SeedRequests(db, rng, bodies);
        db.SaveChanges();
    }

    private static void SeedReferences(SupportPlatformDbContext db)
    {
        foreach (var (code, label) in Domains)
            db.ReferenceDomains.Add(new ReferenceDomain { Code = code, Label = label });
        foreach (var (code, label) in BodyTypes)
            db.ReferenceBodyTypes.Add(new ReferenceBodyType { Code = code, Label = label });
        foreach (var (code, label) in Statuses)
            db.ReferenceStatuses.Add(new ReferenceStatus { Code = code, Label = label });
        foreach (var (code, label) in Districts)
            db.ReferenceDistricts.Add(new ReferenceDistrict { Code = code, Label = label });
    }

    private static void SeedRegistry(SupportPlatformDbContext db)
    {
        db.FilterFieldRegistry.AddRange(
            new FilterFieldRegistryEntry
            {
                Id = "bodyType", Label = "סוג גוף", Kind = "codeList",
                ReferenceList = "bodyTypes", Operators = ["in"], Segmentable = true, SortOrder = 1
            },
            new FilterFieldRegistryEntry
            {
                Id = "supportDomain", Label = "תחום תמיכה", Kind = "codeList",
                ReferenceList = "domains", Operators = ["in"], Segmentable = true, SortOrder = 2
            },
            new FilterFieldRegistryEntry
            {
                Id = "status", Label = "סטטוס", Kind = "codeList",
                ReferenceList = "statuses", Operators = ["in"], Segmentable = false, SortOrder = 3
            },
            new FilterFieldRegistryEntry
            {
                Id = "district", Label = "מחוז", Kind = "codeList",
                ReferenceList = "districts", Operators = ["in"], Segmentable = true, SortOrder = 4
            },
            new FilterFieldRegistryEntry
            {
                Id = "supportYear", Label = "שנת תמיכה", Kind = "yearRange",
                ReferenceList = null, Operators = ["range", "single"], Segmentable = true, SortOrder = 5
            });
    }

    private static Tenant[] SeedTenants(SupportPlatformDbContext db)
    {
        Tenant[] tenants =
        [
            new() { Id = "culture-sport-admin", Name = "מנהל התרבות והספורט" },
            new() { Id = "welfare-admin", Name = "מנהל הרווחה" }
        ];
        db.Tenants.AddRange(tenants);
        return tenants;
    }

    private static void SeedUsers(SupportPlatformDbContext db)
    {
        (string Username, string Tenant, string Role)[] users =
        [
            ("sarah", "culture-sport-admin", "analyst"),
            ("dan", "culture-sport-admin", "admin"),
            ("michal", "welfare-admin", "analyst")
        ];

        foreach (var (username, tenant, role) in users)
        {
            db.Users.Add(new User
            {
                Id = DeterministicGuid($"user:{username}"),
                Username = username,
                PasswordHash = SeedPasswordHasher.Hash(username, DemoPassword),
                TenantId = tenant,
                Role = role
            });
        }
    }

    private static SubmittingBody[] SeedBodies(SupportPlatformDbContext db, Random rng)
    {
        // ~40 bodies, weighted toward the primary tenant.
        (string Tenant, int Count)[] plan =
        [
            ("culture-sport-admin", 28),
            ("welfare-admin", 12)
        ];

        var bodies = new List<SubmittingBody>();
        foreach (var (tenant, count) in plan)
        {
            for (var i = 1; i <= count; i++)
            {
                var type = Pick(rng, BodyTypes).Code;
                var district = Pick(rng, Districts).Code;
                var prefix = type == "association" ? "עמותת" : "חברת";
                bodies.Add(new SubmittingBody
                {
                    Id = DeterministicGuid($"body:{tenant}:{i}"),
                    Name = $"{prefix} דוגמה {tenant[..3]}-{i:D2}",
                    TenantId = tenant,
                    BodyTypeCode = type,
                    DistrictCode = district
                });
            }
        }

        db.SubmittingBodies.AddRange(bodies);
        return [.. bodies];
    }

    private static void SeedRequests(SupportPlatformDbContext db, Random rng, SubmittingBody[] bodies)
    {
        // ~500 requests, weighted per tenant.
        (string Tenant, int Count)[] plan =
        [
            ("culture-sport-admin", 320),
            ("welfare-admin", 180)
        ];

        foreach (var (tenant, count) in plan)
        {
            var tenantBodies = bodies.Where(b => b.TenantId == tenant).ToArray();
            for (var i = 0; i < count; i++)
            {
                var body = tenantBodies[rng.Next(tenantBodies.Length)];
                var year = Weighted(rng, Years, [0.30, 0.40, 0.30]);
                var status = Weighted(rng, Statuses, [0.55, 0.25, 0.20]).Code;
                var domain = Weighted(rng, Domains, [0.60, 0.40]).Code;

                var requested = rng.Next(10, 501) * 1000m;
                var approved = status switch
                {
                    "approved" => decimal.Round(requested * (decimal)(0.6 + rng.NextDouble() * 0.4), 2),
                    _ => 0m
                };

                db.SupportRequests.Add(new SupportRequest
                {
                    Id = DeterministicGuid($"req:{tenant}:{i}"),
                    TenantId = tenant,
                    SubmittingBodyId = body.Id,
                    SupportDomainCode = domain,
                    StatusCode = status,
                    SupportYear = year,
                    AmountRequested = requested,
                    AmountApproved = approved
                });
            }
        }
    }

    private static T Pick<T>(Random rng, T[] items) => items[rng.Next(items.Length)];

    private static T Weighted<T>(Random rng, T[] items, double[] weights)
    {
        var roll = rng.NextDouble();
        var cumulative = 0.0;
        for (var i = 0; i < items.Length; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
                return items[i];
        }

        return items[^1];
    }

    private static Guid DeterministicGuid(string key)
    {
        var hash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(key));
        return new Guid(hash);
    }
}
