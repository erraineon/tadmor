using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using E621;
using Humanizer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using Tadmor.Services.Abstractions;

namespace Tadmor.Services.E621
{
    [SingletonService]
    public class E621Service
    {
        private readonly ChatService _chatService;
        private readonly ILogger<E621Service> _logger;
        private readonly E621Client _client;
        private readonly IDictionary<ulong, PokemonGameSession> _pokemonGameSession = new ConcurrentDictionary<ulong, PokemonGameSession>();

        private const string PokemonNameTagsString =
            @"pokémon_taur, baby_kangaskhan, bulbasaur, ivysaur, venusaur, charmander, charmeleon, charizard, squirtle, wartortle, blastoise, caterpie, metapod, butterfree, weedle, kakuna, pidgey, pidgeotto, pidgeot, rattata, raticate, spearow, fearow, ekans, arbok, pikachu, raichu, sandshrew, sandslash, nidorina, nidoqueen, nidorino, nidoking, clefairy, clefable, vulpix, ninetales, jigglypuff, wigglytuff, zubat, golbat, oddish, gloom_(pokémon), vileplume, paras, parasect, venonat, venomoth, diglett, dugtrio, meowth, persian_(pokémon), psyduck, golduck, mankey, primeape, growlithe, arcanine, poliwag, poliwhirl, poliwrath, abra, kadabra, alakazam, machop, machoke, machamp, bellsprout, weepinbell, victreebel, tentacool, tentacruel, geodude, graveler, ponyta, rapidash, slowpoke, slowbro, magnemite, magneton, farfetch'd, doduo, dodrio, seel, dewgong, grimer, muk, shellder, cloyster, gastly, haunter, gengar, onix, drowzee, krabby, kingler, voltorb, exeggcute, exeggutor, cubone, marowak, hitmonlee, hitmonchan, lickitung, koffing, weezing, rhyhorn, rhydon, chansey, tangela, kangaskhan, horsea, seadra, goldeen, seaking, staryu, starmie, mr._mime, scyther, jynx, electabuzz, magmar, pinsir, tauros, magikarp, gyarados, lapras, ditto_(pokémon), eevee, vaporeon, jolteon, flareon, porygon, omanyte, kabuto, kabutops, aerodactyl, snorlax, articuno, zapdos, moltres, dratini, dragonair, dragonite, mewtwo, mew, chikorita, bayleef, meganium, cyndaquil, quilava, typhlosion, totodile, croconaw, feraligatr, sentret, furret, hoothoot, noctowl, ledyba, ledian, spinarak, ariados, crobat, chinchou, lanturn, pichu, cleffa, igglybuff, togepi, togetic, natu, xatu, mareep, flaaffy, ampharos, bellossom, marill, azumarill, sudowoodo, politoed, hoppip, skiploom, jumpluff, aipom, sunkern, sunflora, yanma, wooper, quagsire, espeon, umbreon, murkrow, slowking, misdreavus, unown_(pokémon), wobbuffet, girafarig, pineco, forretress, dunsparce, gligar, steelix, snubbull, granbull, qwilfish, scizor, shuckle, heracross, sneasel, teddiursa, ursaring, slugma, magcargo, swinub, piloswine, corsola, remoraid, octillery, delibird, mantine, skarmory, houndour, houndoom, kingdra, phanpy, donphan, porygon2, stantler, smeargle, tyrogue, hitmontop, smoochum, elekid, magby, miltank, blissey, raikou, entei, suicune, larvitar, pupitar, tyranitar, lugia, ho-oh, celebi, treecko, grovyle, sceptile, torchic, combusken, blaziken, mudkip, marshtomp, swampert, poochyena, mightyena, zigzagoon, linoone, wurmple, silcoon, beautifly, cascoon, dustox, lotad, lombre, ludicolo, seedot, nuzleaf, shiftry, taillow, swellow, wingull, pelipper, ralts, kirlia, gardevoir, surskit, masquerain, shroomish, breloom, slakoth, vigoroth, slaking, nincada, ninjask, shedinja, whismur, loudred, exploud, makuhita, hariyama, azurill, nosepass, skitty, delcatty, sableye, mawile, aron, lairon, aggron, meditite, medicham, electrike, manectric, plusle, minun, volbeat, illumise, roselia, gulpin, swalot, carvanha, sharpedo, wailmer, wailord, numel, camerupt, torkoal, spoink, grumpig, spinda, trapinch, vibrava, flygon, cacnea, cacturne, swablu, altaria, zangoose, seviper, lunatone, solrock, barboach, whiscash, corphish, crawdaunt, baltoy, claydol, lileep, cradily, anorith, armaldo, feebas, milotic, castform, kecleon, shuppet, banette, duskull, dusclops, tropius, chimecho, absol, wynaut, snorunt, glalie, spheal, sealeo, walrein, clamperl, huntail, gorebyss, relicanth, luvdisc, bagon, shelgon, salamence, beldum, metang, metagross, regirock, regice, registeel, latias, latios, kyogre, groudon, rayquaza, jirachi, deoxys, turtwig, grotle, torterra, chimchar, monferno, infernape, piplup, prinplup, empoleon, starly, staravia, staraptor, bidoof, bibarel, kricketune, shinx, luxio, luxray, budew, roserade, cranidos, rampardos, shieldon, bastiodon, burmy, wormadam, mothim, combee, vespiquen, pachirisu, buizel, floatzel, cherubi, cherrim, shellos, gastrodon, ambipom, drifloon, drifblim, buneary, lopunny, mismagius, honchkrow, glameow, purugly, chingling, stunky, skuntank, bronzor, bronzong, bonsly, mime_jr., happiny, chatot, spiritomb, gible, gabite, garchomp, munchlax, riolu, lucario, hippopotas, hippowdon, skorupi, drapion, croagunk, toxicroak, carnivine, finneon, lumineon, mantyke, snover, abomasnow, weavile, magnezone, lickilicky, rhyperior, tangrowth, electivire, magmortar, togekiss, yanmega, leafeon, glaceon, gliscor, mamoswine, porygon-z, gallade, probopass, dusknoir, froslass, rotom, uxie, mesprit, azelf, dialga, palkia, heatran, regigigas, giratina, cresselia, phione, manaphy, darkrai, shaymin, arceus, victini, snivy, servine, serperior, tepig, pignite, emboar, oshawott, dewott, samurott, patrat, watchog, lillipup, herdier, stoutland, purrloin, liepard, pansage, simisage, pansear, simisear, panpour, simipour, munna, musharna, pidove, tranquill, unfezant, blitzle, zebstrika, roggenrola, boldore, gigalith, woobat, swoobat, drilbur, excadrill, audino, timburr, gurdurr, conkeldurr, tympole, palpitoad, seismitoad, throh, sawk, sewaddle, swadloon, leavanny, venipede, whirlipede, scolipede, cottonee, whimsicott, petilil, lilligant, basculin, sandile, krokorok, krookodile, darumaka, darmanitan, maractus, dwebble, crustle, scraggy, scrafty, sigilyph, yamask, cofagrigus, tirtouga, carracosta, archen, archeops, trubbish, garbodor, zorua, zoroark, minccino, cinccino, gothita, gothorita, gothitelle, solosis, duosion, reuniclus, ducklett, swanna, vanillite, vanillish, vanilluxe, deerling, sawsbuck, emolga, karrablast, escavalier, foongus, amoonguss, frillish, jellicent, alomomola, joltik, galvantula, ferroseed, ferrothorn, klink, klang, klinklang, tynamo, eelektrik, eelektross, elgyem, beheeyem, litwick, lampent, chandelure, axew, fraxure, haxorus, cubchoo, beartic, cryogonal, shelmet, accelgor, stunfisk, mienfoo, mienshao, druddigon, golett, golurk, pawniard, bisharp, bouffalant, rufflet, braviary, vullaby, mandibuzz, heatmor, durant, deino, zweilous, hydreigon, larvesta, volcarona, cobalion, terrakion, virizion, tornadus, thundurus, reshiram, zekrom, landorus, kyurem, keldeo, meloetta, genesect, chespin, quilladin, chesnaught, fennekin, braixen, delphox, froakie, frogadier, greninja, bunnelby, diggersby, fletchling, fletchinder, talonflame, scatterbug, spewpa, vivillon, litleo, pyroar, flabébé, floette, florges, skiddo, gogoat, pancham, pangoro, furfrou, espurr, meowstic, honedge, doublade, aegislash, spritzee, aromatisse, swirlix, slurpuff, inkay, malamar, binacle, barbaracle, skrelp, dragalge, clauncher, clawitzer, helioptile, heliolisk, tyrunt, tyrantrum, amaura, aurorus, sylveon, hawlucha, dedenne, carbink, goomy, sliggoo, goodra, klefki, phantump, trevenant, pumpkaboo, gourgeist, bergmite, avalugg, noibat, noivern, xerneas, yveltal, zygarde, diancie, hoopa, volcanion, rowlet, dartrix, decidueye, litten, torracat, incineroar, popplio, brionne, primarina, pikipek, trumbeak, toucannon, yungoos, gumshoos, grubbin, charjabug, vikavolt, crabrawler, crabominable, oricorio, cutiefly, ribombee, rockruff, lycanroc, wishiwashi, mareanie, toxapex, mudbray, mudsdale, dewpider, araquanid, fomantis, lurantis, morelull, shiinotic, salandit, salazzle, stufful, bewear, bounsweet, steenee, tsareena, comfey, oranguru, passimian, wimpod, golisopod, sandygast, palossand, pyukumuku, type_null, silvally, minior, komala, turtonator, togedemaru, mimikyu, bruxish, drampa, dhelmise, jangmo-o, hakamo-o, kommo-o, tapu_koko, tapu_lele, tapu_bulu, tapu_fini, cosmog, cosmoem, solgaleo, lunala, nihilego, buzzwole, pheromosa, xurkitree, celesteela, kartana, guzzlord, necrozma, magearna, marshadow, ub_burst, ub_assembly, ub_adhesive, alolan_rattata, alolan_raticate, alolan_raichu, alolan_sandshrew, alolan_sandslash, alolan_vulpix, alolan_ninetales, alolan_diglett, alolan_dugtrio, alolan_meowth, alolan_persian, alolan_geodude, alolan_graveler, alolan_golem, alolan_grimer, alolan_muk, alolan_exeggutor, alolan_marowak, mega_venusaur, mega_charizard_x, mega_charizard_y, mega_blastoise, mega_alakazam, mega_gengar, mega_kangaskhan, mega_pinsir, mega_gyarados, mega_aerodactyl, mega_mewtwo_x, mega_mewtwo_y, mega_ampharos, mega_scizor, mega_heracross, mega_houndoom, mega_tyranitar, mega_blaziken, mega_gardevoir, mega_mawile, mega_aggron, mega_medicham, mega_manectric, mega_banette, mega_absol, mega_garchomp, mega_lucario, mega_abomasnow, mega_beedrill, mega_pidgeot, mega_slowbro, mega_steelix, mega_sceptile, mega_swampert, mega_sableye, mega_sharpedo, mega_camerupt, mega_altaria, mega_glalie, mega_salamence, mega_metagross, mega_latias, mega_latios, mega_rayquaza, mega_lopunny, mega_gallade, mega_audino, mega_diancie, primal_groudon, primal_kyogre, golem_(pokémon), zeraora, poipole, naganadel, stakataka, blacephalon, shadow_lugia, meltan, nidoran, werepokémon, grookey, scorbunny, sobble, electrode_(pokémon), hypno_(pokémon), dawn_wings_necrozma, ultra_necrozma, dusk_mane_necrozma, zygarde_complete_forme, zygarde_core, zygarde_10_forme, wooloo, gossifleur, eldegoss, drednaw, corviknight, zacian, zamazenta, yamper, impidimp, gigantamax_drednaw, gigantamax_corviknight, alcremie, gigantamax_alcremie, rolycoly, duraludon, galarian_zigzagoon, galarian_linoone, galarian_weezing, obstagoon, morpeko, cramorant, polteageist, sirfetch'd, galarian_ponyta, gigantamax_charizard, gigantamax_butterfree, gigantamax_meowth, gigantamax_pikachu, gigantamax_eevee, arrokuda, mr._rime, runerigus, appletun, applin, arctovish, arctozolt, barraskewda, blipbug, boltund, carkol, centiskorch, chewtle, cinderace, clobbopus, coalossal, copperajah, galarian_corsola, corvisquire, cufant, cursola, galarian_darmanitan, galarian_darumaka, dottler, dracovish, dracozolt, dragapult, drakloak, dreepy, drizzile, dubwool, eiscue, eternatus, falinks, flapple, frosmoth, gigantamax_centiskorch, gigantamax_coalossal, eternamax_eternatus, gigantamax_flapple, gigantamax_garbodor, gigantamax_grimmsnarl, gigantamax_hatterene, gigantamax_kingler, gigantamax_lapras, gigantamax_machamp, gigantamax_sandaconda, grapploct, greedent, grimmsnarl, hatenna, hatterene, hattrem, indeedee, inteleon, galarian_meowth, milcery, morgrem, galarian_mr._mime, nickit, orbeetle, perrserker, pincurchin, raboot, galarian_rapidash, rillaboom, rookidee, sandaconda, silicobra, sinistea, sizzlipede, skwovet, snom, stonjourner, galarian_stunfisk, thievul, thwackey, toxel, toxtricity, galarian_yamask, galarian_farfetch'd, gigantamax_gengar, gigantamax_snorlax, gigantamax_melmetal, gigantamax_orbeetle, gigantamax_toxtricity, gigantamax_copperajah, gigantamax_duraludon, kubfu, urshifu, calyrex, galarian_slowpoke, galarian_slowbro, galarian_slowking, galarian_articuno, galarian_zapdos, galarian_moltres, gigantamax_venusaur, gigantamax_blastoise, gigantamax_rillaboom, gigantamax_cinderace, gigantamax_inteleon, gigantamax_urshifu, gigantamax_appletun, beta_pokémon_(species), zarude, beedrill, kricketot, omastar, melmetal (learn more).";

        private static readonly HashSet<string> PokemonNameTags = new HashSet<string>(PokemonNameTagsString
                .Split(", ")
                .Select(s => s.Replace("pokémon_", string.Empty).Replace("_(pokémon)", string.Empty)),
            StringComparer.OrdinalIgnoreCase);

        public E621Service(IOptions<E621Options> options, ChatService chatService, ILogger<E621Service> logger)
        {
            _chatService = chatService;
            _logger = logger;
            _client = new E621Client(options.Value.UserAgent);
        }

        public async Task<E621Post> SearchRandom(string tags)
        {
            var options = new E621SearchOptions
            {
                Tags = $"{tags} order:random",
            };
            var posts = await _client.Search(options);
            return posts.Any() ? posts.RandomSubset(1).Single() : throw new Exception("no results");
        }

        public async Task<(List<E621Post>, long)> SearchAfter(string tags, long afterId)
        {
            var searchOptions = new E621SearchOptions
            {
                Tags = tags,
            };
            var posts = await _client.Search(searchOptions);
            var newPosts = posts
                .TakeWhile(post => post.Id > afterId)
                .Take(afterId == default ? 1 : 8)
                .ToList();
            var newLastId = posts.Max(post => post.Id);
            return (newPosts, newLastId);
        }

        public async Task ToggleSession(IMessageChannel channel)
        {
            var channelId = channel.Id;
            if (!_pokemonGameSession.ContainsKey(channelId))
            {
                var session = new PokemonGameSession("pokemon -young -gore -scat");
                _pokemonGameSession[channelId] = session;
                RunSession(session, channel);
            }
            else
            {
                await StopSession(channel);
            }
        }

        private async Task StopSession(IMessageChannel channel)
        {
            if (_pokemonGameSession.Remove(channel.Id, out var session))
                session.CancellationTokenSource.Cancel();
            await channel.SendMessageAsync("the pokemon game was stopped");
        }

        private async void RunSession(PokemonGameSession session, IMessageChannel channel)
        {
            void IncreaseScore(IMessage guessingMessage)
            {
                var guesserId = guessingMessage.Author.Id;
                session.GuildUserScores[guesserId] =
                    session.GuildUserScores.TryGetValue(guesserId, out var currentScore)
                        ? currentScore + 1
                        : 1;
            }

            static string PickRandomTag(E621Post post)
            {
                return post.Tags.Species
                    .Intersect(PokemonNameTags)
                    .RandomSubset(1)
                    .Single();
            }

            async Task<E621Post> GetRandomPostUntilNotNull(string tags)
            {
                E621Post post;
                do post = await SearchRandom(tags);
                while (post?.File?.Url == null);
                return post;
            }

            try
            {
                while (!session.CancellationTokenSource.IsCancellationRequested)
                {
                    var timeoutSource = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                    try
                    {
                        var post = await GetRandomPostUntilNotNull(session.Tags);
                        await channel.SendMessageAsync(post.File.Url);
                        var tag = PickRandomTag(post);
                        var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
                            timeoutSource.Token,
                            session.CancellationTokenSource.Token);

                        bool MatchesSelectedTag(IUserMessage um) => um.Content != null &&
                            um.Channel.Id == channel.Id &&
                            new[] { tag.Humanize(), tag }.Any(s =>
                                  um.Content.Equals(s, StringComparison.OrdinalIgnoreCase));

                        var guessingMessage = await _chatService.Next(MatchesSelectedTag, linkedSource.Token);
                        IncreaseScore(guessingMessage);
                        await channel.SendMessageAsync($"the correct answer was {tag.Humanize()}");
                    }
                    catch (Exception e) when (!(e is TaskCanceledException))
                    {
                        _logger.LogError(e.ToString());
                    }
                }
            }
            catch (TaskCanceledException)
            {
                if (!session.CancellationTokenSource.IsCancellationRequested)
                    await StopSession(channel);
            }
        }
    }
}