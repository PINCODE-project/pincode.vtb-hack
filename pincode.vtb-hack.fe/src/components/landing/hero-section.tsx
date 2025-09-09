import React from "react";
import Link from "next/link";
import Image from "next/image";
import { ArrowRight, Brain } from "lucide-react";
import { AnimatedGroup } from "@/components/ui/animated-group";
import Noise from "@/components/ui/noise";
import { Button } from "@pin-code/ui-kit";
import { Variants } from "framer-motion";

const transitionVariants = {
	item: {
		hidden: {
			opacity: 0,
			filter: "blur(12px)",
			y: 12,
		},
		visible: {
			opacity: 1,
			filter: "blur(0px)",
			y: 0,
			transition: {
				type: "spring",
				bounce: 0.3,
				duration: 1.5,
			},
		},
	} as Variants,
};

export async function HeroSection() {
	return (
		<>
			<main className="overflow-hidden dark">
				<section>
					<div className="relative pt-24 md:!pt-36">
						<AnimatedGroup
							variants={{
								container: {
									visible: {
										transition: {
											delayChildren: 1,
										},
									},
								},
								item: {
									hidden: {
										opacity: 0,
										y: 20,
									},
									visible: {
										opacity: 1,
										y: 0,
										transition: {
											type: "spring",
											bounce: 0.3,
											duration: 2,
										},
									},
								},
							}}
							className="absolute inset-0 -z-20"
						>
							<Image
								src="/images/background.jpg"
								alt="background"
								className="absolute inset-x-0 -z-20"
								width={3276}
								height={4095}
							/>
							<div className="absolute top-0 h-full w-full">
								<Noise
									patternSize={250}
									patternScaleX={1}
									patternScaleY={1}
									patternRefreshInterval={2}
									patternAlpha={15}
								/>
							</div>
						</AnimatedGroup>

						<div className="mx-auto max-w-7xl px-6">
							<div className="text-center sm:mx-auto lg:mr-auto lg:mt-0">
								<AnimatedGroup variants={transitionVariants}>
									<Link
										href="/databases"
										className="group mx-auto flex w-fit items-center gap-4 rounded-full border bg-muted p-1 pl-4 shadow-md shadow-black/5 transition-all duration-300 hover:bg-background dark:border-t-white/5 dark:shadow-zinc-950 dark:hover:border-t-border"
									>
										<Brain width="16" className="stroke-blue-400" />
										<span className="text-sm text-foreground">Умный взгляд на SQL</span>
										<span className="block h-4 w-0.5 border-l bg-white dark:border-background dark:bg-zinc-700"></span>

										<div className="size-6 overflow-hidden rounded-full bg-background duration-500 group-hover:bg-muted">
											<div className="flex w-12 -translate-x-1/2 duration-500 ease-in-out group-hover:translate-x-0">
												<span className="flex size-6">
													<ArrowRight className="m-auto size-3" />
												</span>
												<span className="flex size-6">
													<ArrowRight className="m-auto size-3" />
												</span>
											</div>
										</div>
									</Link>

									<h1 className="mx-auto mt-8 max-w-4xl text-balance text-5xl font-bold sm:text-6xl md:text-7xl lg:mt-16">
										Все SQL-запросы <br />— под контролем
									</h1>
									<p className="mx-auto mt-8 max-w-2xl text-balance text-lg">
										Анализ SQL-запросов и метрик PostgreSQL с проактивным контролем
										производительности и рекомендациями по оптимизации.
									</p>
								</AnimatedGroup>

								<AnimatedGroup
									variants={{
										container: {
											visible: {
												transition: {
													staggerChildren: 0.05,
													delayChildren: 0.75,
												},
											},
										},
										...transitionVariants,
									}}
									className="mt-12 flex flex-col items-center justify-center gap-2 md:flex-row"
								>
									<div key={1} className="rounded-[14px] border bg-foreground/10 p-0.5">
										<Button asChild size="lg" className="rounded-xl px-5 text-base">
											<Link href="/databases">
												<span className="text-nowrap">Начать бесплатно</span>
											</Link>
										</Button>
									</div>
									<Button key={2} asChild size="lg" variant="ghost" className="rounded-xl px-5">
										<Link href="/documentation">
											<span className="text-nowrap">Документация</span>
										</Link>
									</Button>
								</AnimatedGroup>
							</div>
						</div>

						<AnimatedGroup
							variants={{
								container: {
									visible: {
										transition: {
											staggerChildren: 0.05,
											delayChildren: 0.75,
										},
									},
								},
								...transitionVariants,
							}}
						>
							<div className="relative -mr-56 mt-8 overflow-hidden px-2 sm:mr-0 sm:mt-12 md:mt-20">
								<div
									aria-hidden
									className="absolute inset-0 z-10 bg-gradient-to-b from-transparent from-35% to-background"
								/>
								<div className="inset-shadow-2xs dark:inset-shadow-white/20 relative mx-auto max-w-6xl overflow-hidden rounded-2xl border bg-background p-4 shadow-lg shadow-zinc-950/15 ring-1 ring-background">
									<Image
										className="aspect-15/8 relative hidden rounded-2xl bg-background dark:block"
										src="/images/demo.png"
										alt="app screen"
										width="2700"
										height="1440"
									/>
								</div>
							</div>
						</AnimatedGroup>
					</div>
				</section>
			</main>
		</>
	);
}
