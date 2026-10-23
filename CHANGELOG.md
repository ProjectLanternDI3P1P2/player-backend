# Changelog

## [1.4.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/v1.3.0...v1.4.0) (2026-10-23)


### Added

* **auth:** authorize player hub commands ([13a6cdf](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/13a6cdf4ebe25897e8bff6ff4af038e0c315bdff))
* **auth:** link players to external identities ([e3ca636](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/e3ca63627090ed37b1b92699bc86d270ffbd15c3))
* **auth:** resolve players from gateway identity ([19e7a15](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/19e7a15ee6f8af8fc1ee049fc1ecc69c02bd4045))
* **migrations:** add migration-only command for GitOps jobs ([7f1d0f0](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/7f1d0f02b473f8ca79debdf2a6aa8c51f2441ef2))
* **migrations:** add migration-only image command for GitOps ([dc28416](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/dc28416ca8d65389f798881a1939ecdd5af08185))
* **player:** delete a hero (US-PLAYER-10) ([036806e](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/036806e90b9a41386167dbc7d422b63d31ab651a))
* **player:** delete a hero (US-PLAYER-10) ([fe9a01d](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/fe9a01d64f7055fd7b107841c927175502ccb750))
* **player:** namespace HTTP routes by service ([1207235](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/12072353125311ca6d4020f3c564f9aa899b3d33))
* **player:** namespace HTTP routes by service ([186e05d](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/186e05d99ee13ec4040382a5c22192417005ff54))
* **player:** reset development database ([b57b7a6](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/b57b7a6eedf4aed61cbe752d2aefe251271bd545))
* **player:** reset development database ([d68a010](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d68a01004e2cb99702aabe95147f016ea37115c8))
* **player:** seed a stable demo roster ([b6c1fb0](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/b6c1fb065867eebbef8842152325b1c17b93e636))
* **player:** seed a stable demo roster ([bbab886](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/bbab8862b1b3b7cfa0ca98835142ee7a21c4076a))
* **player:** store the appearance chosen at hero creation ([e550c0d](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/e550c0d7207d2bc459ef8b5865088cc62ef65698))
* separate package and local Docker builds ([34310f7](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/34310f7f78bb95c83a3aa37fd4cec7fe5fcbd173))
* separate package and local Docker builds ([11db173](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/11db1734d05dba523c6dbbfa6de5a7b387a3b859))
* **session:** start dungeon runs with replay seeds ([946d650](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/946d6509a2193088ac56008ee5b7e428fbad8324))
* **session:** start dungeon runs with replay seeds ([ff5248f](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/ff5248f2e90e98a26444a46bd3b2704b005adcc5))


### Fixed

* align dungeon client coverage with contract ([31d3040](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/31d3040ecc6b405e147ad6a0aabd881f5202552d))
* **auth:** keep legacy clients authenticated ([5362544](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/53625444333521b9094f6b9c611bec13dda92659))
* **cd:** publish images through groupe7 Harbor domain ([a97e1ca](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/a97e1ca24813c21d44b657a8617b43b256f0b2e0))
* **cd:** publish through Harbor 192.214.203.200:18443 ([fad90b5](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/fad90b58315e1567bf95f2644fb37b5c520687a5))
* **cd:** publish through private Harbor 10.4.111.200:18443 ([9d74232](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/9d74232207e621d4f545d45081df95b12eb7a386))
* **cd:** publish through private Harbor endpoint ([28b0639](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/28b06392c422d6c18035055863ad5cee397102bb))
* **cd:** use shared workflow with new Harbor endpoint ([d65af74](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d65af742df43764dc7c87f361b51caed1511d4f4))
* ci ([d4b936b](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d4b936b2e37e552aaa8c7ae6f0c7ce6e57b28897))
* **ci:** authenticate GitHub Packages restores ([f1b7ef5](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/f1b7ef5b8bb02aca5d73b4c9b20778af1fbe966f))
* **ci:** pin reusable workflow revision ([88a3e98](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/88a3e98900f59a1ab4d89d29675201c8d6722198))
* **player:** lock the hero before its sessions when deleting it ([1f88234](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/1f8823489f880ddeb13ccc5c129e30853225bf53))
* **player:** merge hero appearance with session changes ([5f96911](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/5f969118ab4ad17e16e5c7af7af303d0f15ecefd))
* update contract version ([b278f00](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/b278f0097ecebf2170f632be656f17b0fca4ad26))


### Changed

* **player:** expose the hero's open session as one activeSession object ([c593730](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/c593730592d995fb0544237c34d62317104ddb45))
* **session:** model lifecycle states as enums ([407e314](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/407e314047ff2765ec294ce1b48863abf46a1f9b))

## [1.3.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/v1.2.0...v1.3.0) (2026-10-01)


### Added

* add realtime solo lobby lifecycle ([bff1c84](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/bff1c84257519e1bf7110dd03cf9c615eb20069c))
* allow changing a lobby hero ([4f07f4f](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/4f07f4fd8aeed1b0a322a54c1fc83a6d689a034e))
* configure CORS origins through options ([a9b5524](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/a9b55249f41595e2de4f342d8376b059ac4b4974))
* create game sessions through PlayerHub ([1f2787e](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/1f2787ef73a0ac4b0af730487ebf62cc7f031ecc))


### Fixed

* format ([2471a37](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/2471a37fe576cf3d1d0d567393abb6241d6b1056))
* format and tests ([844f48b](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/844f48b711e98d95ce5a4b0c06929b77d34164b2))
* publish a single changed lobby hero ([4bde892](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/4bde892aff8d86b4777c338c876fb70c4ad47c10))
* use generic SignalR filter registration ([23ed645](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/23ed645cf92c1a0d1148b1308628a0059b90201a))


### Changed

* centralize SignalR command errors ([edb537c](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/edb537c4aafdbe69f24fb41a9013b1789327188b))
* share game session idempotency handling ([00cba9c](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/00cba9c27127310d28f7d742a67dca2efc7628eb))
* share lobby command preparation ([3b70bd2](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/3b70bd27da86044c1e5c081cbb3add1dcd08b181))

## [1.2.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/v1.1.0...v1.2.0) (2026-09-30)


### Added

* **database:** migrate and seed development data ([64a6992](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/64a6992c07c1f7510c39810263a12a688d304c7b))
* **database:** migrate and seed development data ([7c77c8b](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/7c77c8bec35cb6dba37568f0c3ba38297bcc6314))
* **grpc:** propagate correlation IDs ([ff71dde](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/ff71ddebda9e7b6db5b0e72a3142e56a216987cd))
* **grpc:** propagate correlation IDs ([d31b1af](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d31b1af279eff3836148f0ed3f54882094b045b3))
* **hero:** add hero creation endpoint ([95b6af3](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/95b6af31c5ce904cfb713c504dd5f7c9d5a6baad))
* **hero:** add hero sheet endpoint ([07b8027](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/07b802745657d1ab5e41590cdf20392c014e3924))
* **hero:** expose class descriptions ([83a199d](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/83a199d0c870ba6a4b884431c3205332367bdb24))
* **hero:** expose creation date in hero summaries ([65caf7a](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/65caf7a288f9e15c736da58e795e4f2fd631320f))
* **hero:** expose creation date in hero summaries ([9e81fd8](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/9e81fd8c3797902550af8aa0a5be19fca24398fc))
* **hero:** list player heroes ([d1481ad](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d1481ad14419ebfb7b079c824c6a8d00f47878a1))
* **hero:** persist selected hero ([1844d96](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/1844d96ba5740a1bb5dd5ada58b245d0346aad06))
* **openapi:** persist user ID in Scalar ([9ec7b32](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/9ec7b3256facc8f14894a1c4f32cef252c3a78f8))
* **openapi:** persist user ID in Scalar ([f837244](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/f837244c80f77f5e8b929a72fe761a834094fc0c))
* **player:** add hero deselection endpoint ([43b2f8d](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/43b2f8d9e6cc8651b3a5821b7d1e1168af167f68))
* **player:** add hero deselection endpoint ([831021a](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/831021a3f6c2e49fdfb13923c2058452b6eb2fb7))
* **player:** implement player data model persistence ([94062d8](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/94062d8b3558a5245c361ffe3b46407582f9662b))
* **player:** implement player data model persistence ([89244c7](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/89244c7cf6764fae54f99eca78a05e0be9991788))
* **session:** start solo dungeon runs ([de16d5a](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/de16d5abfdd6764daffc86484f1ad21d14bcbfb7))
* **template:** complete database and integration examples ([a3ed989](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/a3ed989b90febd9633a06bcddb733393524ba06d))


### Fixed

* **ci:** harden Docker publishing workflow ([d30f774](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d30f7749bd2ebda2a199d3390b76fa89e52e075f))
* **ci:** provision PostgreSQL for Sonar tests ([125c52a](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/125c52ae5fd0a7e76ff4ada9089b6a19d3ab1b50))
* **database:** add Player initial migration ([3aae4fc](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/3aae4fc2f1afd5c0a76baa653c1ecc1d9d8dcb85))
* **database:** align migration workflow documentation ([1377c11](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/1377c112b8c1e3078eac0966d118789c404c5bb1))
* **database:** retain migration workflow ([791644c](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/791644c7e7669605c7c753b87d1816502d6568e5))
* **docker:** include dockerignore in application project ([fdedfc4](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/fdedfc42df92463c9fed47d76c222c3f11911dde))
* **docker:** include dockerignore in application project ([6014816](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/60148164bcc39252c68ae46fc06e1f96d68d9e84))
* **hero:** complete list summary merge ([930ed1f](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/930ed1f5cdee55a2e1bb39b78a6f754877e73ded))
* **hero:** isolate class listing integration test ([08c8c52](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/08c8c527969a9199aea7bcd813712f775da55a31))

## [1.1.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/v1.0.0...v1.1.0) (2026-09-23)


### Added

* add ef core migration tooling ([b3abca4](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/b3abca46187ee86de0eda40e2da4ef26ef68e275))
* add ef core migration tooling ([e62b3f1](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/e62b3f12e77dce1622db927db6ad55cceaf05166))
* enforce command transaction boundary ([4096387](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/40963875235dde96cbfbe0d22352baf86f1d5255))
* publish player created events through RabbitMQ ([40727b6](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/40727b6125a422e1cbbeefaab2d17520c4ce2650))
* publish player created events through RabbitMQ ([cccf22e](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/cccf22eebfad251aee42150c4ba83fea8879c20c))

## [1.0.0](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/v0.1.1...v1.0.0) (2026-09-22)


### ⚠ BREAKING CHANGES

* **contracts:** release protobuf contracts independently

### Added

* **contracts:** publish protobuf package ([5df7049](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/5df7049dcb876f7f821bc58759f0fef43d7e6ec6))
* **contracts:** release protobuf contracts independently ([d7cefcb](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/d7cefcb8b517075887b9f33c90c4262bac64a212))
* **grpc:** add infrastructure player client ([00a2531](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/00a2531a16d93773e7bed45485eefc608314030a))
* **grpc:** expose player service internally ([b374c80](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/b374c80622924deaa333dabc33f350175f8b1e88))
* Mise en place d'un exemple de service GRPC client et serveur ([550c759](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/550c759e0f5de9d7d8cebb07b2f489016918cb07))

## [0.1.1](https://github.com/ProjectLanternDI3P1P2/player-backend/compare/v0.1.0...v0.1.1) (2026-09-05)


### Fixed

* run the container as the unprivileged app user ([#8](https://github.com/ProjectLanternDI3P1P2/player-backend/issues/8)) ([894be3f](https://github.com/ProjectLanternDI3P1P2/player-backend/commit/894be3fe28e64218726b2dbc2e2bccb7efb9846b))
