import { EventTag } from "$lib/api/cartesian-client";
import {
	Agreement01Icon,
	Airplane01Icon,
	Baby01Icon,
	BabyBottleIcon,
	BalloonsIcon,
	Basketball01Icon,
	BicycleIcon,
	Book01Icon,
	BookOpen01Icon,
	Briefcase01Icon,
	CameraMicrophone01Icon,
	CoPresentIcon,
	ComputerIcon,
	DiceIcon,
	Dress01Icon,
	Dumbbell01Icon,
	Film01Icon,
	Flag01Icon,
	GameController01Icon,
	GiftIcon,
	GraduationScrollIcon,
	HappyIcon,
	Leaf01Icon,
	MoonIcon,
	MountainIcon,
	MusicNote01Icon,
	PaintBoardIcon,
	Restaurant01Icon,
	RunningShoesIcon,
	StarIcon,
	Store01Icon,
	Ticket01Icon,
	UserGroup02Icon,
	UserGroupIcon,
} from "@hugeicons/core-free-icons";

export interface EventTagConfig {
	translationKey: string;
	icon: any;
}

export const EVENT_TAG_CONFIG: Record<EventTag, EventTagConfig> = {
	[EventTag.Outdoor]: {
		translationKey: "tag_outdoor",
		icon: MountainIcon,
	},
	[EventTag.Sport]: {
		translationKey: "tag_sport",
		icon: Basketball01Icon,
	},
	[EventTag.Fitness]: {
		translationKey: "tag_fitness",
		icon: Dumbbell01Icon,
	},
	[EventTag.Literature]: {
		translationKey: "tag_literature",
		icon: Book01Icon,
	},
	[EventTag.Business]: {
		translationKey: "tag_business",
		icon: Briefcase01Icon,
	},
	[EventTag.Tech]: {
		translationKey: "tag_tech",
		icon: ComputerIcon,
	},
	[EventTag.Educational]: {
		translationKey: "tag_educational",
		icon: GraduationScrollIcon,
	},
	[EventTag.Kids]: {
		translationKey: "tag_kids",
		icon: Baby01Icon,
	},
	[EventTag.Family]: {
		translationKey: "tag_family",
		icon: UserGroupIcon,
	},
	[EventTag.Parenting]: {
		translationKey: "tag_parenting",
		icon: BabyBottleIcon,
	},
	[EventTag.Conference]: {
		translationKey: "tag_conference",
		icon: CoPresentIcon,
	},
	[EventTag.Film]: {
		translationKey: "tag_film",
		icon: Film01Icon,
	},
	[EventTag.Fashion]: {
		translationKey: "tag_fashion",
		icon: Dress01Icon,
	},
	[EventTag.Running]: {
		translationKey: "tag_running",
		icon: RunningShoesIcon,
	},
	[EventTag.Cycling]: {
		translationKey: "tag_cycling",
		icon: BicycleIcon,
	},
	[EventTag.BoardGames]: {
		translationKey: "tag_board_games",
		icon: DiceIcon,
	},
	[EventTag.VideoGames]: {
		translationKey: "tag_video_games",
		icon: GameController01Icon,
	},
	[EventTag.Entertainment]: {
		translationKey: "tag_entertainment",
		icon: Ticket01Icon,
	},
	[EventTag.Comedy]: {
		translationKey: "tag_comedy",
		icon: HappyIcon,
	},
	[EventTag.Arts]: {
		translationKey: "tag_arts",
		icon: PaintBoardIcon,
	},
	[EventTag.Hobby]: {
		translationKey: "tag_hobby",
		icon: StarIcon,
	},
	[EventTag.Party]: {
		translationKey: "tag_party",
		icon: BalloonsIcon,
	},
	[EventTag.Gathering]: {
		translationKey: "tag_gathering",
		icon: UserGroup02Icon,
	},
	[EventTag.Charity]: {
		translationKey: "tag_charity",
		icon: GiftIcon,
	},
	[EventTag.Volunteering]: {
		translationKey: "tag_volunteering",
		icon: Agreement01Icon,
	},
	[EventTag.Environmental]: {
		translationKey: "tag_environmental",
		icon: Leaf01Icon,
	},
	[EventTag.Festival]: {
		translationKey: "tag_festival",
		icon: MusicNote01Icon,
	},
	[EventTag.Concert]: {
		translationKey: "tag_concert",
		icon: CameraMicrophone01Icon,
	},
	[EventTag.Food]: {
		translationKey: "tag_food",
		icon: Restaurant01Icon,
	},
	[EventTag.Travel]: {
		translationKey: "tag_travel",
		icon: Airplane01Icon,
	},
	[EventTag.Religious]: {
		translationKey: "tag_religious",
		icon: MoonIcon,
	},
	[EventTag.Study]: {
		translationKey: "tag_study",
		icon: BookOpen01Icon,
	},
	[EventTag.Market]: {
		translationKey: "tag_market",
		icon: Store01Icon,
	},
	[EventTag.Political]: {
		translationKey: "tag_political",
		icon: Flag01Icon,
	},
};
