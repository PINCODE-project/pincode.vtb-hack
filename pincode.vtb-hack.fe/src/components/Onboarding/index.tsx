"use client";
import { WarpOnboarding, WarpOnboardingContent } from "@/components";
import React, { useEffect } from "react";
import { STEPS } from "@components/Onboarding/data.tsx";
import { useOnboardingStore } from "@store/onboarding";
import { useShallow } from "zustand/react/shallow";

export const Onboarding = () => {
	const { isOpen, setIsOpen } = useOnboardingStore(useShallow((s) => s));

	const handleOnboardingComplete = () => {
		localStorage.setItem("feature_databases-onboarding", "false");
	};

	useEffect(() => {
		const hasSeenOnboarding = localStorage.getItem("feature_databases-onboarding");
		if (hasSeenOnboarding !== "false") {
			const timer = setTimeout(() => {
				setIsOpen(true);
			}, 500);

			return () => clearTimeout(timer);
		}
	}, [setIsOpen]);

	return (
		<WarpOnboarding
			steps={STEPS}
			featureId="databases-onboarding-internal"
			open={isOpen}
			onOpenChange={setIsOpen}
			onComplete={handleOnboardingComplete}
			onSkip={handleOnboardingComplete}
			showProgressBar={true}
		>
			<WarpOnboardingContent />
		</WarpOnboarding>
	);
};
