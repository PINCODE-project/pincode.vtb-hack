import { Metadata } from "next";
import { Button } from "@pin-code/ui-kit";
import { Providers } from "@/components";
import Link from "next/link";
import { Glitchy404 } from "@components/NotFound";
import { ThemeToggle } from "@components/theme-toggle.tsx";

export const metadata: Metadata = {
	title: "Страница не найдена - 404",
	description: "Запрашиваемая страница не существует. Вернитесь на главную страницу AI Репетитора.",
	robots: {
		index: false,
		follow: false,
	},
};

export default function NotFound() {
	return (
		<body className={"relative h-full font-sans antialiased"}>
			<Providers>
				<main className={"flex flex-col items-center justify-center gap-4"}>
					<Glitchy404 />
					<h1 className="sr-only">404</h1>
					<h2 className="scroll-m-20 text-center text-4xl font-extrabold tracking-tight text-balance">
						Страница не найдена
					</h2>
					<p className="text-muted-foreground text-xl text-center max-w-[500px]">
						К сожалению, запрашиваемая страница не существует или была перемещена.
					</p>
					<Link href="/">
						<Button>На главную</Button>
					</Link>
					<ThemeToggle />
				</main>
			</Providers>
		</body>
	);
}
