import { DependencyContainer } from "tsyringe";
import type { IPreAkiLoadMod } from "@spt-aki/models/external/IPreAkiLoadMod";
import type {DynamicRouterModService} from "@spt-aki/services/mod/dynamicRouter/DynamicRouterModService";
import { ProfileHelper } from "@spt-aki/helpers/ProfileHelper";
import { ISkinRequestData } from "./ISkinRequestData"
import { IVoiceRequestData } from "./IVoiceRequestData";

class SkinService implements IPreAkiLoadMod
{
    public preAkiLoad(container: DependencyContainer): void 
    {   
        const dynamicRouterModService = container.resolve<DynamicRouterModService>("DynamicRouterModService");
        const profileHelper = container.resolve<ProfileHelper>("ProfileHelper");

        dynamicRouterModService.registerDynamicRouter(
            "DynamicSkinServiceRoute",
            [
                {
                    url: "/skin-service/pmc/change",
                    action: (url, info : ISkinRequestData, sessionId) => 
                    {
                        const pmcData = profileHelper.getPmcProfile(sessionId);

                        pmcData.Customization.Body = info.bodyId;
                        pmcData.Customization.Feet = info.feetId;
                        pmcData.Customization.Head = info.headId;
                        pmcData.Customization.Hands = info.handsId;

                        return JSON.stringify({response: "OK"});
                    }
                },
                {
                    url: "/skin-service/pmc/voice/change",
                    action: (url, info : IVoiceRequestData, sessionId) => 
                    {
                        const pmcData = profileHelper.getPmcProfile(sessionId);

                        pmcData.Info.Voice = info.voiceId;

                        return JSON.stringify({response: "OK"});
                    }
                },
                {
                    url: "/skin-service/scav/change",
                    action: (url, info : ISkinRequestData, sessionId) => 
                    {
                        const scavData = profileHelper.getScavProfile(sessionId);

                        scavData.Customization.Body = info.bodyId;
                        scavData.Customization.Feet = info.feetId;
                        scavData.Customization.Head = info.headId;
                        scavData.Customization.Hands = info.handsId;

                        return JSON.stringify({response: "OK"});
                    }
                },
                {
                    url: "/skin-service/scav/voice/change",
                    action: (url, info : IVoiceRequestData, sessionId) => 
                    {
                        const scavData = profileHelper.getScavProfile(sessionId);

                        scavData.Info.Voice = info.voiceId;

                        return JSON.stringify({response: "OK"});
                    }
                }
            ],
            "skin-service"
        );
    }
}

module.exports = { mod: new SkinService() }