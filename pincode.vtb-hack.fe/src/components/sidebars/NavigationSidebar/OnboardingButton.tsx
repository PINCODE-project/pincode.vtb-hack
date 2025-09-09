import { Button } from "@pin-code/ui-kit";
import React from "react";
import { useOnboardingStore } from "@store/onboarding";
import { CircleQuestionMark } from "lucide-react";

export const OnboardingButton = () => {
	const setIsOpen = useOnboardingStore((s) => s.setIsOpen);
	return (
		<Button
			variant="ghost"
			size="icon"
			onClick={() => {
				setIsOpen(true);
			}}
		>
			<CircleQuestionMark className="h-[1.2rem] w-[1.2rem]" />
		</Button>
	);
};
