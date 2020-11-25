using System.Threading;
using System;
using System.Linq;
using System.Speech.Recognition;
using System.Collections.Generic;
using Rage;
using LSPD_First_Response.Mod.Callouts;
using System.Windows.Forms;

[assembly: Rage.Attributes.Plugin("SpeechRecognitionInject", Author = "radholm", Description = "Speech-to-text translate")]

public static class EntryPoint
{
    private static bool speechActive = false;

    internal static void Main()
    {
        GameFiber.StartNew(delegate
        {
            SpeechRecognitionEngine _recognizer = new SpeechRecognitionEngine();
            _recognizer.LoadGrammarAsync(CreateGrammar());
            _recognizer.SpeechRecognized += _recognizer_SpeechRecognized;
            _recognizer.SpeechRecognitionRejected += _recognizer_speechRecognitionRejected;
            _recognizer.SpeechHypothesized += _recognizer_SpeechHypothesized;

            Game.DisplayNotification($"~b~SRI~w~ (~g~0.1a~w~) by radholm has been loaded successfully");
            while(true)
            {
                if(Game.IsKeyDown(Keys.N))
                {
                    speechActive = true;
                    Game.DisplayHelp($"Listening...", true);
                    _recognizer.SetInputToDefaultAudioDevice(); 
                    _recognizer.RecognizeAsync(RecognizeMode.Multiple);
                }
                else if(!Game.IsKeyDownRightNow(Keys.N) && speechActive)
                {
                    speechActive = false;
                    Game.DisplayHelp($"Stopped");
                    _recognizer.RecognizeAsyncCancel();
                    _recognizer.RecognizeAsyncStop();
                }
                GameFiber.Yield();
            }
        });

        void _recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //Game.DisplaySubtitle('"' + e.Result.Text + "." + '"');
            Game.DisplayNotification("~g~" + e.Result.Text);
            SpawnVehicle(e.Result.Text.Split(' ')[1]);
        }

        void _recognizer_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Game.DisplayNotification("~y~" + e.Result.Text);
        }

        void _recognizer_speechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Game.DisplayNotification("~r~No match found");

        }
    }

    internal static Grammar CreateGrammar()
    {
        string[] modelNames = "adder,airbus,airtug,akuma,ambulance,annihilator,armytanker,armytrailer,armytrailer2,asea,asea2,asterope,bagger,baletrailer,baller,baller2,banshee,barracks,barracks2,bati,bati2,benson,bfinjection,biff,bison,bison2,bison3,bjxl,blazer,blazer2,blazer3,blimp,blista,bmx,boattrailer,bobcatxl,bodhi2,boxville,boxville2,boxville3,buccaneer,buffalo,buffalo2,bulldozer,bullet,burrito,burrito2,burrito3,burrito4,burrito5,bus,buzzard,buzzard2,cablecar,caddy,caddy2,camper,carbonizzare,carbonrs,cargobob,cargobob2,cargobob3,cargoplane,cavalcade,cavalcade2,cheetah,coach,cogcabrio,comet2,coquette,cruiser,crusader,cuban800,cutter,daemon,dilettante,dilettante2,dinghy,dinghy2,dloader,docktrailer,docktug,dominator,double,dubsta,dubsta2,dump,dune,dune2,duster,elegy2,emperor,emperor2,emperor3,entityxf,exemplar,f620,faggio2,fbi,fbi2,felon,felon2,feltzer2,firetruk,fixter,flatbed,forklift,fq2,freight,freightcar,freightcont1,freightcont2,freightgrain,freighttrailer,frogger,frogger2,fugitive,fusilade,futo,gauntlet,gburrito,graintrailer,granger,gresley,habanero,handler,hauler,hexer,hotknife,infernus,ingot,intruder,issi2,jackal,jb700,jet,jetmax,journey,khamelion,landstalker,lazer,lguard,luxor,mammatus,manana,marquis,maverick,mesa,mesa2,mesa3,metrotrain,minivan,mixer,mixer2,monroe,mower,mule,mule2,nemesis,ninef,ninef2,oracle,oracle2,packer,patriot,pbus,pcj,penumbra,peyote,phantom,phoenix,picador,police,police2,police3,police4,policeb,policeold1,policeold2,policet,polmav,pony,pony2,pounder,prairie,pranger,predator,premier,primo,proptrailer,radi,raketrailer,rancherxl,rancherxl2,rapidgt,rapidgt2,ratloader,rebel,rebel2,regina,rentalbus,rhino,riot,ripley,rocoto,romero,rubble,ruffian,ruiner,rumpo,rumpo2,sabregt,sadler,sadler2,sanchez,sanchez2,sandking,sandking2,schafter2,schwarzer,scorcher,scrap,seashark,seashark2,seminole,sentinel,sentinel2,serrano,shamal,sheriff,sheriff2,skylift,speedo,speedo2,squalo,stanier,stinger,stingergt,stockade,stockade3,stratum,stretch,stunt,submersible,sultan,suntrap,superd,surano,surfer,surfer2,surge,taco,tailgater,tanker,tankercar,taxi,tiptruck,tiptruck2,titan,tornado,tornado2,tornado3,tornado4,tourbus,towtruck,towtruck2,tr2,tr3,tr4,tractor,tractor2,tractor3,trailerlogs,trailers,trailers2,trailers3,trailersmall,trash,trflat,tribike,tribike2,tribike3,tropic,tvtrailer,utillitruck,utillitruck2,utillitruck3,vacca,vader,velum,vigero,voltic,voodoo2,washington,youga,zion,zion2,ztype,bifta,kalahari,paradise,speeder,btype,jester,turismor,alpha,vestra,massacro,zentorno,huntley,thrust,rhapsody,warrener,blade,glendale,panto,dubsta3,pigalle,monster,sovereign,besra,miljet,coquette2,swift,innovat,hakuchou,furoregt,jester2,massacro2,ratloader2,slamvan,mule3,velum2,tanker2,casco,boxville4,hydra,insurgent,insurgent2,gburrito2,technical,dinghy3,savage,enduro,guardian,lectro,kuruma,kuruma2,trash2,barracks3,valkyrie,slamvan2,marshall,dukes2,Blista3,Blista2,dodo,submersible2,buffalo3,dukes,stalion,stalion2,dominator2,gauntlet2,blimp2".ToUpper().Split(',');
        GrammarBuilder vehicles = new GrammarBuilder(new Choices(modelNames));

        GrammarBuilder spawnPhrase = new GrammarBuilder("Spawn");
        spawnPhrase.Append(vehicles);

        GrammarBuilder givePhrase = new GrammarBuilder("Give");
        givePhrase.Append("weapon");

        Choices allChoices = new Choices(new GrammarBuilder[] { spawnPhrase, givePhrase });
        Grammar g = new Grammar((GrammarBuilder)allChoices);

        return g;
    }

    internal static void SpawnVehicle(string vehicleModel)
    {
        GameFiber.StartNew(delegate
        {
            new Vehicle(vehicleModel, Game.LocalPlayer.Character.GetOffsetPosition(Vector3.RelativeFront * 10f), Game.LocalPlayer.Character.Heading);
        });
    }
}
