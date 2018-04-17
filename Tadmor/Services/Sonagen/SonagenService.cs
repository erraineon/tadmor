using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;
using Microsoft.Extensions.Options;
using Tadmor.Extensions;

namespace Tadmor.Services.Sonagen
{
    public class SonagenService
    {
        private readonly SonagenOptions _options;

        public SonagenService(IOptions<SonagenOptions> options)
        {
            _options = options.Value;
        }

        public Sona GenerateSona(Random random)
        {
            var gender = _options.Genders.Random(g => g.Weight, random);
            var pronouns = gender.Pronouns.ToLower().Split('/');
            var species = _options.Species.Random(s => s.Weight, random);
            var attributesAndGroups = _options.AttributeGroups
                .SelectMany(group => Enumerable.Repeat(group, random.Next(group.Max) + 1))
                .OrderBy(_ => random.Next())
                .Where(g => (float) random.NextDouble() < g.Weight)
                .Select(g => (g, a: g.Attributes.Random(a => a.Weight, random)))
                .ToList();

            //ok to put it outside of format because it only occurs one time
            var speciesPool = _options.Species.Cast<SonaWeightedObject>().ToList();

            string Format(string s)
            {
                var formatted = Regex.Replace(s, @"{([\w\s]+)}", match =>
                {
                    var attributeGroupsPool = _options.AttributeGroups.Select(g =>
                        (attributes: g.Attributes.Cast<SonaWeightedObject>().ToList(), value: g.Value)).ToList();
                    var matchedGroup = match.Groups[1].Value;
                    switch (matchedGroup)
                    {
                        case "He":
                            return pronouns[0];
                        case "His":
                            return pronouns[1];
                        case "Him":
                            return pronouns[2];
                        default:
                            var pool = matchedGroup == "Species"
                                ? speciesPool
                                : attributeGroupsPool.Single(g => g.value == matchedGroup).attributes;
                            pool.RemoveAll(o => o.Value == s);
                            var replacement = pool.ToList().Random(a => a.Weight, random).Value;
                            return Format(replacement);
                    }
                });
                return formatted.ToLower();
            }

            var sona = new Sona
            {
                Gender = Format(gender.Value),
                Species = Format(species.Value),
                AttributesByGroup = attributesAndGroups
                    .ToLookup(t => t.g.Value.ToLower(), t => (Format(t.a.Value), t.a.Type))
            };
            var attributesByType = sona.AttributesByGroup.SelectMany(g => g).ToLookup(a => a.type, a => a.value);
            var adjectives = attributesByType[AttributeType.Adjective].Prepend(sona.Gender);
            var possessives = attributesByType[AttributeType.Possessive].ToList();
            var relatives = attributesByType[AttributeType.Relative].ToList();
            var relativePos = attributesByType[AttributeType.RelativePossessive].ToList();

            var builder = new StringBuilder();
            builder.Append(string.Join(", ", adjectives));
            builder.Append($" {sona.Species}");
            if (possessives.Any())
            {
                builder.Append($" with {possessives.Humanize()}");
            }
            else if (relativePos.Any())
            {
                builder.Append($" whose {relativePos.Humanize()}");
                relativePos.Clear();
            }
            else if (relatives.Any())
            {
                builder.Append($" who {relatives.Humanize()}");
                relatives.Clear();
            }

            if (relatives.Any()) builder.Append($". {pronouns[0]} {relatives.Humanize()}");
            if (relativePos.Any()) builder.Append($". {pronouns[1]} {relativePos.Humanize()}");
            sona.Description = builder.ToString();
            return sona;
        }
    }
}