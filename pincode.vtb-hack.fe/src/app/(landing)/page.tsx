import { HeroHeader } from "@components/landing/header.tsx";
import { HeroSection } from "@components/landing/hero-section.tsx";

/**
 * Главная страница приложения
 */
export default async function Home() {
	return (
		<>
			<HeroHeader />
			<HeroSection />
		</>
	);
}
